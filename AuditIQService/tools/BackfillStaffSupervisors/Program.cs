using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Data.SqlClient;

// Backfills User.SupervisorId from Scorebuddy's raw "staff" export in
// AuditIQ.Staging.StagingRecords (Entity='staff') — a flat array of staff records per row,
// each carrying its own supervisor_id (person-to-person, not team-to-owner: Scorebuddy's
// Team object itself has no supervisor field). Matches staff to Users via LegacySourceId
// (format: staff:{staff_id}). Idempotent — safe to re-run.

var stagingConnStr = Environment.GetEnvironmentVariable("STAGING_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set STAGING_CONNECTION_STRING (points at AuditIQ.Staging).");
var destConnStr = Environment.GetEnvironmentVariable("DEST_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set DEST_CONNECTION_STRING (points at the target AuditIQ app database, e.g. AuditIQ.Dev).");
var dryRun = Environment.GetEnvironmentVariable("DRY_RUN") == "1";

Console.WriteLine($"Dry run: {dryRun}");

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// staffId -> supervisorId (both Scorebuddy staff_id, nullable). Last-write-wins is fine here:
// the same staff_id appearing in multiple staging batches always carries the same supervisor_id
// (verified: identical content on re-appearance, just paginated capture artifacts).
var supervisorByStaffId = new Dictionary<int, int?>();

await using (var stagingConn = new SqlConnection(stagingConnStr))
{
    await stagingConn.OpenAsync();
    await using var cmd = new SqlCommand("SELECT Id, Payload FROM StagingRecords WHERE Entity = 'staff' ORDER BY Id;", stagingConn);
    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        var stagingId = reader.GetInt32(0);
        var payload = reader.GetString(1);

        List<StaffPayload>? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<List<StaffPayload>>(payload, jsonOptions);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"  SKIP staging row {stagingId}: failed to parse JSON ({ex.Message})");
            continue;
        }

        foreach (var staff in parsed ?? [])
            supervisorByStaffId[staff.StaffId] = staff.SupervisorId;
    }
}

Console.WriteLine($"Parsed {supervisorByStaffId.Count} distinct staff records from staging JSON.");

var rows = supervisorByStaffId
    .Where(kv => kv.Value is not null)
    .Select(kv => (EmployeeLegacyId: $"staff:{kv.Key}", SupervisorLegacyId: $"staff:{kv.Value}"))
    .ToList();

Console.WriteLine($"{rows.Count} staff record(s) have a non-null supervisor_id.");

if (rows.Count == 0)
{
    Console.WriteLine("Nothing to backfill.");
    return;
}

await using var destConn = new SqlConnection(destConnStr);
await destConn.OpenAsync();

await using (var createTemp = new SqlCommand(
    """
    CREATE TABLE #SupervisorLinks (
        EmployeeLegacyId nvarchar(400) NOT NULL PRIMARY KEY,
        SupervisorLegacyId nvarchar(400) NOT NULL
    );
    """, destConn))
{
    await createTemp.ExecuteNonQueryAsync();
}

using (var bulk = new SqlBulkCopy(destConn) { DestinationTableName = "#SupervisorLinks" })
{
    var table = new System.Data.DataTable();
    table.Columns.Add("EmployeeLegacyId", typeof(string));
    table.Columns.Add("SupervisorLegacyId", typeof(string));
    foreach (var row in rows)
        table.Rows.Add(row.EmployeeLegacyId, row.SupervisorLegacyId);
    await bulk.WriteToServerAsync(table);
}

await using (var unmatchedEmployeeCmd = new SqlCommand(
    """
    SELECT COUNT(*) FROM #SupervisorLinks l
    LEFT JOIN Users u ON u.LegacySourceId = l.EmployeeLegacyId
    WHERE u.Id IS NULL;
    """, destConn))
{
    var count = (int)(await unmatchedEmployeeCmd.ExecuteScalarAsync())!;
    Console.WriteLine($"Supervisor links whose employee has no matching User in destination: {count}");
}

await using (var unmatchedSupervisorCmd = new SqlCommand(
    """
    SELECT COUNT(*) FROM #SupervisorLinks l
    JOIN Users u ON u.LegacySourceId = l.EmployeeLegacyId
    LEFT JOIN Users s ON s.LegacySourceId = l.SupervisorLegacyId
    WHERE s.Id IS NULL;
    """, destConn))
{
    var count = (int)(await unmatchedSupervisorCmd.ExecuteScalarAsync())!;
    Console.WriteLine($"Supervisor links whose supervisor has no matching User in destination (e.g. supervisor never migrated/deleted): {count}");
}

if (dryRun)
{
    Console.WriteLine("Dry run — no rows updated.");
    return;
}

await using var updateCmd = new SqlCommand(
    """
    UPDATE u
    SET u.SupervisorId = s.Id
    FROM Users u
    JOIN #SupervisorLinks l ON l.EmployeeLegacyId = u.LegacySourceId
    JOIN Users s ON s.LegacySourceId = l.SupervisorLegacyId
    WHERE u.Id <> s.Id;
    """, destConn); // guard against a staff record listing itself as its own supervisor
updateCmd.CommandTimeout = 120;
var updated = await updateCmd.ExecuteNonQueryAsync();
Console.WriteLine($"Updated {updated} Users rows.");

sealed class StaffPayload
{
    [JsonPropertyName("staff_id")]
    public int StaffId { get; set; }

    [JsonPropertyName("supervisor_id")]
    public int? SupervisorId { get; set; }
}
