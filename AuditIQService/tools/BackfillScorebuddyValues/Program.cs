using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;

// Backfills QuestionAnswerOption.Value / IsFailAll / IsFailSection / IsNotApplicable for
// scorecards migrated from Scorebuddy. The schema rework that added those columns
// (migration 20260902043958_AddScorecardScoringAndGroups) defaulted every existing row to
// 0/false, so every migrated scorecard scored 0% until this backfill runs. The real values
// were never lost — they're sitting in AuditIQ.Staging.StagingRecords (Entity='scorecards'),
// captured verbatim from the Scorebuddy API during migration. This tool reads that JSON and
// writes the real per-answer values back onto the matching QuestionAnswerOption rows, joined
// by LegacySourceId (format: scorecard:{id}:version:{v}:question:{qid}:answer:{key}).
//
// Idempotent — safe to re-run. Matches only rows whose LegacySourceId is legacy-tagged, so it
// never touches scorecards/versions created directly through the app's own builder UI.

var stagingConnStr = Environment.GetEnvironmentVariable("STAGING_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set STAGING_CONNECTION_STRING (points at AuditIQ.Staging).");
var destConnStr = Environment.GetEnvironmentVariable("DEST_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set DEST_CONNECTION_STRING (points at the target AuditIQ app database, e.g. AuditIQ.Dev).");
var dryRun = Environment.GetEnvironmentVariable("DRY_RUN") == "1";

Console.WriteLine($"Dry run: {dryRun}");

// The staging payload mixes conventions: the outer wrapper ("Scorecard", "Versions") is
// PascalCase from the .NET migration tool that captured it, but everything inside is raw
// snake_case straight from the Scorebuddy REST API — hence explicit [JsonPropertyName]
// attributes below on every inner field rather than a single blanket naming policy.
var JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

var candidates = new List<(string LegacyId, int ScorecardId, int VersionNumber, decimal RawValue, decimal Value, bool IsFailAll, bool IsFailSection, bool IsNotApplicable)>();

// Almost every migrated scorecard version scores answers on a 0-1 fractional scale (answer_value
// is "fraction of this question's weight earned"), which maps cleanly onto our 0-100 Value
// column. Some versions instead carry out-of-range answer_values — either isolated data-entry
// typos (e.g. one "-5" amid an otherwise clean version) or, in a few cases, a wholesale
// different additive point system (e.g. Pass=+2, Fail=-4). Either way, multiplying by 100
// would produce nonsensical values, so exclude the affected (scorecard, version) from the
// automatic backfill rather than guess at a conversion — scoped per-version, since sibling
// versions of the same scorecard are independent scoring configs and are often unaffected
// (verified: scorecard 195 v2 is clean and matches 12 real historical evaluation scores
// exactly; only v1 has a stray bad value).
var versionsWithOutOfScaleValues = new HashSet<(int ScorecardId, int VersionNumber)>();

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
            parsed = JsonSerializer.Deserialize<List<ScorecardPayload>>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"  SKIP staging row {stagingId}: failed to parse JSON ({ex.Message})");
            continue;
        }

        var entry = parsed?.SingleOrDefault();
        if (entry?.Scorecard is null || entry.Versions is null) continue;

        var scorecardId = entry.Scorecard.ScorecardId;
        foreach (var version in entry.Versions)
        {
            foreach (var question in version.Questions ?? [])
            {
                foreach (var answer in question.Answers ?? [])
                {
                    var legacyId = $"scorecard:{scorecardId}:version:{version.VersionNumber}:question:{question.QuestionId}:answer:{answer.AnswerKey}";

                    if (Math.Abs(answer.AnswerValue) > 1)
                    {
                        versionsWithOutOfScaleValues.Add((scorecardId, version.VersionNumber));
                        if (Environment.GetEnvironmentVariable("DIAGNOSE_SCALE") == "1")
                            Console.WriteLine($"OUT-OF-SCALE scorecard={scorecardId} version={version.VersionNumber} question={question.QuestionId} \"{question.Question}\" answer=\"{answer.AnswerLabel}\" raw={answer.AnswerValue}");
                    }

                    // A Fail Section / Fail All / N/A answer always scores zero in our schema
                    // (the cascade already zeroes its effective contribution, and N/A is excluded
                    // from scoring entirely) — normalize to 0 here regardless of what Scorebuddy
                    // stored (e.g. N/A commonly carried answer_value=1) so re-saving the scorecard
                    // through our builder later doesn't trip CreateScorecardCommandValidator's
                    // "Fail/N-A answers must be zero" rule.
                    var isSpecial = answer.PointOfFailure || answer.FailSection || answer.NotApplicable;
                    var value = isSpecial ? 0m : Math.Round(answer.AnswerValue * 100m, 2);

                    candidates.Add((legacyId, scorecardId, version.VersionNumber, answer.AnswerValue, value, answer.PointOfFailure, answer.FailSection, answer.NotApplicable));
                }
            }
        }
    }
}

if (versionsWithOutOfScaleValues.Count > 0)
{
    Console.WriteLine($"Excluding {versionsWithOutOfScaleValues.Count} scorecard version(s) with a non-fractional (non 0-1) answer scale — needs a manual scoring-model decision, not an automatic *100 conversion:");
    foreach (var (id, v) in versionsWithOutOfScaleValues.OrderBy(x => x.ScorecardId).ThenBy(x => x.VersionNumber))
        Console.WriteLine($"  scorecard {id} version {v}");
}

if (Environment.GetEnvironmentVariable("DIAGNOSE_SCALE") == "1")
{
    Console.WriteLine("Diagnose-scale mode — exiting before touching the destination database.");
    return;
}

// The staging payload has a small number of exact-duplicate question entries within the same
// version (verified: identical question text/weighting/answers, e.g. scorecard 103 question
// 17090) — a scrape artifact, not conflicting data. Group by LegacySourceId and only collapse
// duplicates that agree on every field; anything that disagrees is a real data problem and
// must not be silently resolved by picking one arbitrarily.
var byLegacyId = candidates
    .Where(c => !versionsWithOutOfScaleValues.Contains((c.ScorecardId, c.VersionNumber)))
    .GroupBy(c => c.LegacyId)
    .ToList();

var conflicting = byLegacyId
    .Where(g => g.Select(c => (c.Value, c.IsFailAll, c.IsFailSection, c.IsNotApplicable)).Distinct().Count() > 1)
    .ToList();

if (conflicting.Count > 0)
{
    Console.WriteLine($"ABORTING: {conflicting.Count} LegacySourceId(s) have conflicting duplicate values in staging JSON — needs manual review, not an automatic pick:");
    foreach (var g in conflicting.Take(20))
        Console.WriteLine($"  {g.Key}: {string.Join(" vs ", g.Select(c => $"[Value={c.Value},FailAll={c.IsFailAll},FailSection={c.IsFailSection},NA={c.IsNotApplicable}]"))}");
    return;
}

var duplicateCount = byLegacyId.Count(g => g.Count() > 1);
if (duplicateCount > 0)
    Console.WriteLine($"Collapsed {duplicateCount} exact-duplicate LegacySourceId(s) (identical values — safe to dedupe).");

var rows = byLegacyId
    .Select(g => g.First())
    .Select(c => new BackfillRow(c.LegacyId, c.Value, c.IsFailAll, c.IsFailSection, c.IsNotApplicable))
    .ToList();

Console.WriteLine($"Parsed {candidates.Count} answer rows from staging JSON; {rows.Count} eligible for backfill.");

if (rows.Count == 0)
{
    Console.WriteLine("Nothing to backfill.");
    return;
}

await using var destConn = new SqlConnection(destConnStr);
await destConn.OpenAsync();

await using (var createTemp = new SqlCommand(
    """
    CREATE TABLE #Backfill (
        LegacySourceId nvarchar(400) NOT NULL PRIMARY KEY,
        Value decimal(5,2) NOT NULL,
        IsFailAll bit NOT NULL,
        IsFailSection bit NOT NULL,
        IsNotApplicable bit NOT NULL
    );
    """, destConn))
{
    await createTemp.ExecuteNonQueryAsync();
}

using (var bulk = new SqlBulkCopy(destConn) { DestinationTableName = "#Backfill" })
{
    var table = new System.Data.DataTable();
    table.Columns.Add("LegacySourceId", typeof(string));
    table.Columns.Add("Value", typeof(decimal));
    table.Columns.Add("IsFailAll", typeof(bool));
    table.Columns.Add("IsFailSection", typeof(bool));
    table.Columns.Add("IsNotApplicable", typeof(bool));

    foreach (var row in rows)
        table.Rows.Add(row.LegacySourceId, row.Value, row.IsFailAll, row.IsFailSection, row.IsNotApplicable);

    await bulk.WriteToServerAsync(table);
}

await using (var unmatchedCmd = new SqlCommand(
    """
    SELECT COUNT(*) FROM #Backfill b
    LEFT JOIN QuestionAnswerOptions o ON o.LegacySourceId = b.LegacySourceId
    WHERE o.Id IS NULL;
    """, destConn))
{
    var unmatched = (int)(await unmatchedCmd.ExecuteScalarAsync())!;
    Console.WriteLine($"Staging answers with no matching QuestionAnswerOption in destination: {unmatched}");
}

await using (var uncoveredCmd = new SqlCommand(
    """
    SELECT COUNT(*) FROM QuestionAnswerOptions o
    WHERE o.LegacySourceId LIKE 'scorecard:%'
      AND NOT EXISTS (SELECT 1 FROM #Backfill b WHERE b.LegacySourceId = o.LegacySourceId);
    """, destConn))
{
    var uncovered = (int)(await uncoveredCmd.ExecuteScalarAsync())!;
    Console.WriteLine($"Destination legacy-tagged options with no matching staging answer: {uncovered}");
}

if (dryRun)
{
    Console.WriteLine("Dry run — no rows updated.");
    return;
}

await using var updateCmd = new SqlCommand(
    """
    UPDATE o
    SET o.Value = b.Value,
        o.IsFailAll = b.IsFailAll,
        o.IsFailSection = b.IsFailSection,
        o.IsNotApplicable = b.IsNotApplicable
    FROM QuestionAnswerOptions o
    JOIN #Backfill b ON o.LegacySourceId = b.LegacySourceId;
    """, destConn);
updateCmd.CommandTimeout = 120;
var updated = await updateCmd.ExecuteNonQueryAsync();
Console.WriteLine($"Updated {updated} QuestionAnswerOptions rows.");

record BackfillRow(string LegacySourceId, decimal Value, bool IsFailAll, bool IsFailSection, bool IsNotApplicable);

sealed class ScorecardPayload
{
    public ScorecardHeader? Scorecard { get; set; }
    public List<VersionPayload>? Versions { get; set; }
}

sealed class ScorecardHeader
{
    [JsonPropertyName("scorecard_id")]
    public int ScorecardId { get; set; }
}

sealed class VersionPayload
{
    [JsonPropertyName("version_number")]
    public int VersionNumber { get; set; }

    [JsonPropertyName("questions")]
    public List<QuestionPayload>? Questions { get; set; }
}

sealed class QuestionPayload
{
    [JsonPropertyName("question_id")]
    public int QuestionId { get; set; }

    [JsonPropertyName("question")]
    public string? Question { get; set; }

    [JsonPropertyName("answers")]
    public List<AnswerPayload>? Answers { get; set; }
}

sealed class AnswerPayload
{
    [JsonPropertyName("answer_key")]
    public int AnswerKey { get; set; }

    [JsonPropertyName("answer_label")]
    public string? AnswerLabel { get; set; }

    [JsonPropertyName("answer_value")]
    public decimal AnswerValue { get; set; }

    [JsonPropertyName("point_of_failure")]
    public bool PointOfFailure { get; set; }

    [JsonPropertyName("fail_section")]
    public bool FailSection { get; set; }

    [JsonPropertyName("not_applicable")]
    public bool NotApplicable { get; set; }
}
