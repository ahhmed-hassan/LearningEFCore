using _10_Code_first.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace _10_Code_first.Examples;

/// <summary>
/// Demonstrates the queryability problem with computed (non-persisted) properties
/// on owned value objects, and three alternatives to work around it.
///
/// Setup:
///   - Section has an owned DateRange (Schedule) with persisted StartDate and EndDate.
///   - DateRange also has computed properties: TotalDays, IsActive, Contains(date).
///   - These computed properties are Ignore()'d in the EF configuration — they have no column.
///
/// The problem:
///   Querying by Schedule.TotalDays, Schedule.IsActive, or Schedule.Contains(date)
///   throws InvalidOperationException because EF cannot translate C# logic to SQL
///   when there is no corresponding column.
///
/// Alternatives demonstrated:
///   A) Rewrite queries using persisted properties (StartDate, EndDate) — best approach
///   B) HasComputedColumnSql — let SQL Server compute the value as a real column (explained, not run)
///   C) Client-side evaluation with AsEnumerable() — last resort, loads all rows
/// </summary>
public static class ComputedPropertyExamples
{
    /// <summary>
    /// Runs all computed property examples in sequence.
    /// </summary>
    public static async Task RunAll(ILogger logger)
    {
        logger.LogInformation("\n=== Computed Property Queryability Examples ===");
        logger.LogInformation("DateRange has persisted StartDate/EndDate and computed TotalDays, IsActive, Contains()\n");

        await Example5_ComputedProperty_FailsToTranslate(logger);
        await Example6_RewriteWithPersistedProperties(logger);
        Example7_ComputedColumnSql_Explained(logger);
        await Example8_ClientSideEvaluation(logger);
    }

    /// <summary>
    /// THE KEY DEMONSTRATION: querying a computed property on an owned entity FAILS.
    /// EF Core cannot translate Schedule.TotalDays or Schedule.Contains() to SQL
    /// because these are C# computations, not mapped columns.
    /// </summary>
    private static async Task Example5_ComputedProperty_FailsToTranslate(ILogger logger)
    {
        logger.LogInformation("--- Example 5: Computed Property — Fails to Translate ---");

        using var context = new AppDbContext();

        // Attempt 1: Query using computed property TotalDays
        logger.LogInformation("Attempting: Where(s => s.Schedule.TotalDays > 60)...");
        try
        {
            var longSections = await context.Sections
                .Where(s => s.Schedule.TotalDays > 60)
                .ToListAsync();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("FAILED (TotalDays): {Message}",
                ex.Message.Length > 200 ? ex.Message[..] + "..." : ex.Message);
        }

        // Attempt 2: Query using the Contains() method
        var today = DateTime.Now;
        logger.LogInformation("Attempting: Where(s => s.Schedule.Contains(today))...");
        try
        {
            var activeSections = await context.Sections
                .Where(s => s.Schedule.Contains(today))
                .ToListAsync();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("FAILED (Contains): {Message}",
                ex.Message.Length > 200 ? ex.Message[..] + "..." : ex.Message);
        }

        // Attempt 3: Query using IsActive
        logger.LogInformation("Attempting: Where(s => s.Schedule.IsActive)...");
        try
        {
            var activeSections = await context.Sections
                .Where(s => s.Schedule.IsActive)
                .ToListAsync();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("FAILED (IsActive): {Message}",
                ex.Message.Length > 200 ? ex.Message[..] + "..." : ex.Message);
        }

        logger.LogInformation("All three failed — EF cannot translate computed properties to SQL.\n");
    }

    /// <summary>
    /// ALTERNATIVE A: Rewrite the query using only persisted properties.
    /// Instead of Schedule.Contains(date), write: Schedule.StartDate &lt;= date &amp;&amp; date &lt;= Schedule.EndDate.
    /// Instead of Schedule.TotalDays > N, use: EF.Functions.DateDiffDay(StartDate, EndDate) > N.
    /// These translate to SQL because StartDate and EndDate ARE mapped columns.
    /// </summary>
    private static async Task Example6_RewriteWithPersistedProperties(ILogger logger)
    {
        logger.LogInformation("--- Example 6: Alternative A — Rewrite with Persisted Properties ---");

        using var context = new AppDbContext();
        var today = DateTime.Now;

        // Instead of Schedule.Contains(today), rewrite with persisted props
        // Translates to: WHERE Schedule_StartDate <= @today AND @today <= Schedule_EndDate
        var activeSections = await context.Sections
            .Where(s => s.Schedule.StartDate <= today && today <= s.Schedule.EndDate)
            .ToListAsync();

        logger.LogInformation("Active sections (rewrite of Contains): {Count}", activeSections.Count);
        foreach (var s in activeSections)
        {
            logger.LogInformation("  - {Name}: {Start:d} to {End:d} ({Days} days)",
                s.SectionName, s.Schedule.StartDate, s.Schedule.EndDate, s.Schedule.TotalDays);
        }

        // Instead of Schedule.TotalDays > 60, use DateDiffDay on the persisted columns
        // Translates to: WHERE DATEDIFF(DAY, Schedule_StartDate, Schedule_EndDate) > 60
        var longSections = await context.Sections
            .Where(s => EF.Functions.DateDiffDay(s.Schedule.StartDate, s.Schedule.EndDate) > 60)
            .ToListAsync();

        logger.LogInformation("Sections longer than 60 days (DateDiffDay): {Count}", longSections.Count);
        foreach (var s in longSections)
        {
            logger.LogInformation("  - {Name}: {Days} days",
                s.SectionName, s.Schedule.TotalDays);
        }

        logger.LogInformation("");
    }

    // =====================================================================================
    // ALTERNATIVE B: HasComputedColumnSql (Explained — Not Run)
    // =====================================================================================
    //
    // Instead of Ignore()'ing a computed property, you can MAP it to a SQL-computed column.
    // SQL Server computes the value on-the-fly from other columns in the same row.
    // EF Core sees it as a real column, so LINQ queries on it translate to SQL.
    //
    // HOW IT WORKS:
    //
    //   In your entity configuration (e.g., SectionConfiguration.cs), instead of:
    //
    //       scheduleBuilder.Ignore(d => d.TotalDays);
    //
    //   You would write:
    //
    //       scheduleBuilder.Property(d => d.TotalDays)
    //           .HasColumnName("Schedule_TotalDays")
    //           .HasComputedColumnSql("DATEDIFF(DAY, Schedule_StartDate, Schedule_EndDate)");
    //
    //   This tells EF Core:
    //     1. TotalDays IS a column (so LINQ can reference it in WHERE/ORDER BY/etc.)
    //     2. Its value is computed by SQL Server using the DATEDIFF expression
    //     3. EF will never try to INSERT or UPDATE this column — SQL Server owns it
    //
    //   After adding a migration (Add-Migration AddScheduleTotalDaysComputed), the migration
    //   would generate:
    //
    //       migrationBuilder.AddColumn<int>(
    //           name: "Schedule_TotalDays",
    //           table: "Sections",
    //           computedColumnSql: "DATEDIFF(DAY, Schedule_StartDate, Schedule_EndDate)");
    //
    //   Then this query WOULD work:
    //
    //       var longSections = await context.Sections
    //           .Where(s => s.Schedule.TotalDays > 60)   // <-- now translates to SQL!
    //           .ToListAsync();
    //
    //       // Generated SQL:
    //       // SELECT ... FROM [Sections] WHERE [Schedule_TotalDays] > 60
    //
    // WHY WE DON'T RUN IT HERE:
    //
    //   You cannot both Ignore() and map a property in the same configuration. We chose to
    //   Ignore() TotalDays so that Example 5 can demonstrate the failure. If you want to
    //   use HasComputedColumnSql, you'd replace the Ignore() call with the Property() call
    //   above, then create a new migration.
    //
    // TRADE-OFFS:
    //
    //   Pros:
    //     - Query translates to SQL (server-side filtering, indexable)
    //     - No need to rewrite LINQ queries — use the property naturally
    //     - SQL Server recomputes on every read, so it's always up-to-date
    //
    //   Cons:
    //     - Couples your C# property to a SQL expression — the formula lives in two places
    //     - The C# getter (EndDate - StartDate).Days and the SQL DATEDIFF(DAY, ...) must
    //       stay in sync manually. If one changes, the other must too.
    //     - Only works for expressions that SQL Server can compute from the same row's columns
    //     - Cannot use for cross-table aggregations (e.g., tasks.Sum(t => t.EstimatedTime))
    //
    //   For cross-entity computed values (like EndDate = StartDate + SUM of child durations),
    //   HasComputedColumnSql won't help. You'd need either:
    //     - A database VIEW or stored procedure
    //     - Alternative A (rewrite the LINQ to join/aggregate explicitly)
    //     - Alternative C (client-side evaluation)
    //
    // =====================================================================================

    /// <summary>
    /// ALTERNATIVE B explanation only — logs what HasComputedColumnSql would do.
    /// See the detailed block comment above for the full explanation and code samples.
    /// </summary>
    private static void Example7_ComputedColumnSql_Explained(ILogger logger)
    {
        logger.LogInformation("--- Example 7: Alternative B — HasComputedColumnSql (Explained) ---");
        logger.LogInformation("Instead of Ignore(d => d.TotalDays), you can map it as a computed column:");
        logger.LogInformation("  scheduleBuilder.Property(d => d.TotalDays)");
        logger.LogInformation("      .HasColumnName(\"Schedule_TotalDays\")");
        logger.LogInformation("      .HasComputedColumnSql(\"DATEDIFF(DAY, Schedule_StartDate, Schedule_EndDate)\");");
        logger.LogInformation("This creates a real SQL column — EF can query it: WHERE Schedule_TotalDays > 60");
        logger.LogInformation("Trade-off: the formula lives in both C# and SQL, and must be kept in sync.");
        logger.LogInformation("See ComputedPropertyExamples.cs for detailed documentation.\n");
    }

    /// <summary>
    /// ALTERNATIVE C: Load all rows client-side, then filter using C# logic.
    /// Works with ANY computed property or method, but loads ALL rows from the DB first.
    /// Bad for performance on large tables — use only as a last resort.
    /// </summary>
    private static async Task Example8_ClientSideEvaluation(ILogger logger)
    {
        logger.LogInformation("--- Example 8: Alternative C — Client-Side with AsEnumerable ---");

        using var context = new AppDbContext();
        var today = DateTime.Now;

        // AsEnumerable() forces EF to load ALL rows, then filter in C#
        // The SQL is just: SELECT * FROM Sections (no WHERE clause!)
        var activeSections = context.Sections
            .AsEnumerable()  // everything after this runs in C#, not SQL
            .Where(s => s.Schedule.Contains(today))
            .ToList();

        logger.LogInformation("Active sections (client-side Contains): {Count}", activeSections.Count);
        foreach (var s in activeSections)
        {
            logger.LogInformation("  - {Name}: {Start:d} to {End:d}",
                s.SectionName, s.Schedule.StartDate, s.Schedule.EndDate);
        }

        logger.LogInformation("WARNING: This loaded ALL {Total} sections from DB, then filtered in C#.",
            await context.Sections.CountAsync());
        logger.LogInformation("");
    }
}
