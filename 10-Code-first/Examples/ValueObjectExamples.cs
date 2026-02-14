using _10_Code_first.Data;
using _10_Code_first.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace _10_Code_first.Examples;

/// <summary>
/// Demonstrates EF Core value object patterns and their queryability:
/// - Owned entities (OwnsOne) — fully queryable server-side
/// </summary>
public static class ValueObjectExamples
{
    /// <summary>
    /// Shows that queries on owned entity properties translate to SQL.
    /// EF Core maps Address.City to the Address_City column, so WHERE works server-side.
    /// </summary>
    public static async Task Example1_OwnedEntity_QueriesTranslateToSql(ILogger logger)
    {
        logger.LogInformation("\n=== Example 1: Owned Entity — Queries Translate to SQL ===");

        using var context = new AppDbContext();

        // Query by a property of the owned entity (Address.City)
        // This translates to: WHERE Address_City = 'Cairo'
        var cairoInstructors = await context.Instructors
            .Where(i => i.Address != null && i.Address.City == "Cairo")
            .ToListAsync();

        logger.LogInformation("Instructors in Cairo: {Count}", cairoInstructors.Count);
        foreach (var instructor in cairoInstructors)
        {
            logger.LogInformation("  - {FirstName} {LastName} at {Street}, {City}",
                instructor.FirstName, instructor.LastName,
                instructor.Address!.Street, instructor.Address.City);
        }

        // Query by multiple owned properties — also translates to SQL
        var cairoMainSt = await context.Instructors
            .Where(i => i.Address != null
                && i.Address.City == "Cairo"
                && i.Address.Street.Contains("Main"))
            .ToListAsync();

        logger.LogInformation("Instructors on Main St in Cairo: {Count}", cairoMainSt.Count);
    }

    /// <summary>
    /// Shows that owned entities are ALWAYS loaded with their owner.
    /// No .Include() needed — they are part of the same table.
    /// </summary>
    public static async Task Example2_OwnedEntity_AutomaticallyLoaded(ILogger logger)
    {
        logger.LogInformation("\n=== Example 2: Owned Entity — Loaded Automatically (No Include) ===");

        using var context = new AppDbContext();

        // No .Include() — Address is loaded automatically because it's an owned type
        var instructor = await context.Instructors.FirstAsync(i => i.Id == 1);

        logger.LogInformation("Loaded instructor: {FirstName} {LastName}",
            instructor.FirstName, instructor.LastName);

        if (instructor.Address != null)
        {
            logger.LogInformation("Address loaded automatically: {Street}, {City} {ZipCode}",
                instructor.Address.Street, instructor.Address.City, instructor.Address.ZipCode);
        }

        // Instructor without address — Address is null, no error
        var instructorNoAddress = await context.Instructors.FirstAsync(i => i.Id == 4);
        logger.LogInformation("Instructor {FirstName} has address: {HasAddress}",
            instructorNoAddress.FirstName,
            instructorNoAddress.Address != null ? "Yes" : "No (null)");
    }
}
