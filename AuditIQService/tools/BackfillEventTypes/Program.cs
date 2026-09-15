using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;

// Backfills scorecard-scoped EventType/EventSubType rows from the Scorebuddy migration staging
// data. Each scorecard's raw API payload (AuditIQ.Staging.StagingRecords, Entity='scorecards')
// carries its own "Events" tree — event types and sub-event types configured specifically for
// that scorecard, never shared across scorecards (confirmed: the same-named event on two
// scorecards has two distinct Scorebuddy ids and independent sub-event lists). AuditIQ's
// EventType/EventSubType entities used to be a flat global list with no scorecard link at all;
// migration 20260914111231_ScopeEventTypesToScorecard added EventType.ScorecardId and wiped the
// old ungrounded rows — this tool is what repopulates them correctly.
//
// Idempotent — matches by LegacySourceId, safe to re-run. A scorecard with an empty Events array
// in Scorebuddy (e.g. a document/process-review scorecard with no call-type taxonomy) simply
// gets no event types, which is correct: event type is optional per evaluation.

var stagingConnStr = Environment.GetEnvironmentVariable("STAGING_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set STAGING_CONNECTION_STRING (points at AuditIQ.Staging).");
var destConnStr = Environment.GetEnvironmentVariable("DEST_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set DEST_CONNECTION_STRING (points at the target AuditIQ app database, e.g. AuditIQ.Dev).");
var dryRun = Environment.GetEnvironmentVariable("DRY_RUN") == "1";

Console.WriteLine($"Dry run: {dryRun}");

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

var eventTypeRows = new List<(string LegacySourceId, string ScorecardLegacyId, string Name)>();
var eventSubTypeRows = new List<(string LegacySourceId, string EventTypeLegacyId, string Name)>();
var skippedNoEvents = 0;

await using (var stagingConn = new SqlConnection(stagingConnStr))
{
    await stagingConn.OpenAsync();
    await using var cmd = new SqlCommand("SELECT Id, Payload FROM StagingRecords WHERE Entity = 'scorecards' ORDER BY Id;", stagingConn);
    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        var stagingId = reader.GetInt32(0);
        var payload = reader.GetString(1);

        List<ScorecardPayload>? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<List<ScorecardPayload>>(payload, jsonOptions);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"  SKIP staging row {stagingId}: failed to parse JSON ({ex.Message})");
            continue;
        }

        var entry = parsed?.SingleOrDefault();
        if (entry?.Scorecard is null) continue;

        var scorecardId = entry.Scorecard.ScorecardId;
        var currentVersion = entry.Scorecard.CurrentVersion;
        var scorecardLegacyId = $"scorecard:{scorecardId}:version:{currentVersion}";

        var events = (entry.Events ?? []).Where(e => !e.Deleted && !string.IsNullOrWhiteSpace(e.EventName)).ToList();
        if (events.Count == 0)
        {
            skippedNoEvents++;
            continue;
        }

        foreach (var ev in events)
        {
            var eventTypeLegacyId = $"scorecard:{scorecardId}:event:{ev.EventId}";
            eventTypeRows.Add((eventTypeLegacyId, scorecardLegacyId, ev.EventName!.Trim()));

            foreach (var sub in (ev.SubEvents ?? []).Where(s => !s.Deleted && !string.IsNullOrWhiteSpace(s.SubEventName)))
            {
                var subLegacyId = $"{eventTypeLegacyId}:subevent:{sub.SubEventId}";
                eventSubTypeRows.Add((subLegacyId, eventTypeLegacyId, sub.SubEventName!.Trim()));
            }
        }
    }
}

Console.WriteLine($"Scorecards with no usable Events in staging: {skippedNoEvents}");
Console.WriteLine($"Parsed {eventTypeRows.Count} event type rows, {eventSubTypeRows.Count} event sub-type rows.");

var duplicateEventTypeIds = eventTypeRows.GroupBy(r => r.LegacySourceId).Where(g => g.Count() > 1).ToList();
if (duplicateEventTypeIds.Count > 0)
{
    Console.WriteLine($"ABORTING: {duplicateEventTypeIds.Count} duplicate event-type LegacySourceId(s) in staging JSON — needs manual review:");
    foreach (var g in duplicateEventTypeIds.Take(20))
        Console.WriteLine($"  {g.Key}: {string.Join(" vs ", g.Select(r => r.Name))}");
    return;
}

if (eventTypeRows.Count == 0)
{
    Console.WriteLine("Nothing to backfill.");
    return;
}

// The staging scrape has a small number of exact-duplicate event/sub-event name entries under
// the same parent (a scrape artifact, same pattern already documented in
// BackfillScorebuddyValues for duplicate questions) — collapse rather than let the DB's unique
// (parent, Name) index reject the insert. Only the LegacySourceId of the surviving row is lost;
// nothing user-facing changes.
var dedupedEventTypeNameCount = 0;
eventTypeRows = eventTypeRows
    .GroupBy(r => (r.ScorecardLegacyId, r.Name))
    .Select(g => { if (g.Count() > 1) dedupedEventTypeNameCount++; return g.First(); })
    .ToList();
if (dedupedEventTypeNameCount > 0)
    Console.WriteLine($"Collapsed {dedupedEventTypeNameCount} same-name event type duplicate(s) within a scorecard.");

var dedupedSubTypeNameCount = 0;
eventSubTypeRows = eventSubTypeRows
    .GroupBy(r => (r.EventTypeLegacyId, r.Name))
    .Select(g => { if (g.Count() > 1) dedupedSubTypeNameCount++; return g.First(); })
    .ToList();
if (dedupedSubTypeNameCount > 0)
    Console.WriteLine($"Collapsed {dedupedSubTypeNameCount} same-name sub-event duplicate(s) within an event type.");

await using var destConn = new SqlConnection(destConnStr);
await destConn.OpenAsync();

// Resolve each row's target Scorecard.Id by its LegacySourceId (current version only — event
// types belong to the scorecard as a whole, not a specific historical version).
var scorecardIdByLegacyId = new Dictionary<string, Guid>();
await using (var scorecardCmd = new SqlCommand("SELECT Id, LegacySourceId FROM Scorecards WHERE LegacySourceId IS NOT NULL AND IsCurrentVersion = 1;", destConn))
await using (var scorecardReader = await scorecardCmd.ExecuteReaderAsync())
{
    while (await scorecardReader.ReadAsync())
        scorecardIdByLegacyId[scorecardReader.GetString(1)] = scorecardReader.GetGuid(0);
}

var resolvedEventTypes = new List<(Guid Id, string LegacySourceId, Guid ScorecardId, string Name)>();
var unmatchedScorecards = new HashSet<string>();
foreach (var row in eventTypeRows)
{
    if (scorecardIdByLegacyId.TryGetValue(row.ScorecardLegacyId, out var scorecardGuid))
        resolvedEventTypes.Add((Guid.NewGuid(), row.LegacySourceId, scorecardGuid, row.Name));
    else
        unmatchedScorecards.Add(row.ScorecardLegacyId);
}

if (unmatchedScorecards.Count > 0)
{
    Console.WriteLine($"Scorecards in staging with event data but no matching current-version Scorecard in destination: {unmatchedScorecards.Count}");
    foreach (var id in unmatchedScorecards.Take(10))
        Console.WriteLine($"  {id}");
}

var eventTypeIdByLegacyId = resolvedEventTypes.ToDictionary(e => e.LegacySourceId, e => e.Id);
var resolvedSubTypes = new List<(Guid Id, string LegacySourceId, Guid EventTypeId, string Name)>();
var unmatchedEventTypes = 0;
foreach (var row in eventSubTypeRows)
{
    if (eventTypeIdByLegacyId.TryGetValue(row.EventTypeLegacyId, out var eventTypeGuid))
        resolvedSubTypes.Add((Guid.NewGuid(), row.LegacySourceId, eventTypeGuid, row.Name));
    else
        unmatchedEventTypes++;
}

if (unmatchedEventTypes > 0)
    Console.WriteLine($"Sub-event rows skipped (parent event type wasn't resolved): {unmatchedEventTypes}");

Console.WriteLine($"Resolved {resolvedEventTypes.Count} event types and {resolvedSubTypes.Count} sub-types against the destination database.");

await using (var existingCmd = new SqlCommand("SELECT LegacySourceId FROM EventTypes WHERE LegacySourceId IS NOT NULL;", destConn))
await using (var existingReader = await existingCmd.ExecuteReaderAsync())
{
    var existing = new HashSet<string>();
    while (await existingReader.ReadAsync())
        existing.Add(existingReader.GetString(0));

    var alreadyPresent = resolvedEventTypes.Count(e => existing.Contains(e.LegacySourceId));
    if (alreadyPresent > 0)
    {
        Console.WriteLine($"{alreadyPresent} event types already present (from a prior run) — skipping those, inserting the rest.");
        resolvedEventTypes = resolvedEventTypes.Where(e => !existing.Contains(e.LegacySourceId)).ToList();
    }
}

var keepEventTypeIds = resolvedEventTypes.Select(e => e.Id).ToHashSet();
resolvedSubTypes = resolvedSubTypes.Where(s => keepEventTypeIds.Contains(s.EventTypeId)).ToList();

Console.WriteLine($"After skipping already-backfilled rows: {resolvedEventTypes.Count} event types, {resolvedSubTypes.Count} sub-types to insert.");

if (dryRun)
{
    Console.WriteLine("Dry run — no rows written.");
    return;
}

if (resolvedEventTypes.Count == 0)
{
    Console.WriteLine("Nothing new to insert.");
    return;
}

await using (var transaction = (SqlTransaction)await destConn.BeginTransactionAsync())
{
    foreach (var e in resolvedEventTypes)
    {
        await using var insertCmd = new SqlCommand(
            "INSERT INTO EventTypes (Id, ScorecardId, Name, LegacySourceId) VALUES (@id, @scorecardId, @name, @legacyId);",
            destConn, transaction);
        insertCmd.Parameters.AddWithValue("@id", e.Id);
        insertCmd.Parameters.AddWithValue("@scorecardId", e.ScorecardId);
        insertCmd.Parameters.AddWithValue("@name", e.Name);
        insertCmd.Parameters.AddWithValue("@legacyId", e.LegacySourceId);
        await insertCmd.ExecuteNonQueryAsync();
    }

    foreach (var s in resolvedSubTypes)
    {
        await using var insertCmd = new SqlCommand(
            "INSERT INTO EventSubTypes (Id, EventTypeId, Name, LegacySourceId) VALUES (@id, @eventTypeId, @name, @legacyId);",
            destConn, transaction);
        insertCmd.Parameters.AddWithValue("@id", s.Id);
        insertCmd.Parameters.AddWithValue("@eventTypeId", s.EventTypeId);
        insertCmd.Parameters.AddWithValue("@name", s.Name);
        insertCmd.Parameters.AddWithValue("@legacyId", s.LegacySourceId);
        await insertCmd.ExecuteNonQueryAsync();
    }

    await transaction.CommitAsync();
}

Console.WriteLine($"Inserted {resolvedEventTypes.Count} EventTypes and {resolvedSubTypes.Count} EventSubTypes.");

sealed class ScorecardPayload
{
    public ScorecardHeader? Scorecard { get; set; }
    public List<EventPayload>? Events { get; set; }
}

sealed class ScorecardHeader
{
    [JsonPropertyName("scorecard_id")]
    public int ScorecardId { get; set; }

    [JsonPropertyName("current_version")]
    public int CurrentVersion { get; set; }
}

sealed class EventPayload
{
    [JsonPropertyName("event_id")]
    public int EventId { get; set; }

    [JsonPropertyName("event_name")]
    public string? EventName { get; set; }

    [JsonPropertyName("sub_events")]
    public List<SubEventPayload>? SubEvents { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
}

sealed class SubEventPayload
{
    [JsonPropertyName("sub_event_id")]
    public int SubEventId { get; set; }

    [JsonPropertyName("sub_event_name")]
    public string? SubEventName { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
}
