using _10_Code_first.Data;
using _10_Code_first.Entites;
using _10_Code_first.Entites.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace _10_Code_first.Examples;

/// <summary>
/// Demonstrates EF Core value object patterns and their queryability:
/// - Owned entities (OwnsOne) — fully queryable server-side
/// - Value converters (HasConversion) — single column, equality queryable
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

    /// <summary>
    /// Shows that equality queries on value-converted properties translate to SQL.
    /// EF Core uses the converter to turn CourseTag.Advanced into "ADVANCED" in the WHERE clause.
    /// </summary>
    public static async Task Example3_ValueConverter_EqualityQueryWorks(ILogger logger)
    {
        logger.LogInformation("\n=== Example 3: Value Converter — Equality Query Translates to SQL ===");

        using var context = new AppDbContext();

        // Equality comparison: CourseTag gets converted to its string value in SQL
        // Translates to: WHERE Tag = 'ADVANCED'
        var advancedCourses = await context.Courses
            .Where(c => c.Tag == CourseTag.Advanced)
            .ToListAsync();

        logger.LogInformation("Advanced courses: {Count}", advancedCourses.Count);
        foreach (var course in advancedCourses)
        {
            logger.LogInformation("  - {Name} (Tag: {Tag}, Price: {Price:C})",
                course.CourseName, course.Tag, course.Price);
        }

        // Another equality query with a different tag
        var beginnerCourses = await context.Courses
            .Where(c => c.Tag == CourseTag.Beginner)
            .ToListAsync();

        logger.LogInformation("Beginner courses: {Count}", beginnerCourses.Count);
    }

    /// <summary>
    /// Shows what DOESN'T work with value converters.
    /// Calling methods or accessing properties on the value object inside Where
    /// may fail to translate — EF Core only knows about the converted column value.
    /// </summary>
    public static async Task Example4_ValueConverter_Limitations(ILogger logger)
    {
        logger.LogInformation("\n=== Example 4: Value Converter — Limitations ===");

        using var context = new AppDbContext();

        // This works: comparing the Tag column directly using the converter
        var count = await context.Courses
            .CountAsync(c => c.Tag.Equals( CourseTag.Advanced));
        logger.LogInformation("Count of advanced courses (equality works): {Count}", count);

        // Limitation: accessing .Value on the converted property inside a query
        // EF Core sees Tag as a single converted column — it doesn't know about .Value
        try
        {
            var result = await context.Courses
                .Where(c => c.Tag.Value.StartsWith("ADV"))
                .ToListAsync();

            logger.LogInformation("StartsWith query returned: {Count}", result.Count);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Expected failure — cannot access .Value inside query: {Message}",
                ex.Message.Length > 150 ? ex.Message[..150] + "..." : ex.Message);
        }

        // Workaround: use raw SQL or filter client-side when needed
        logger.LogInformation("Workaround: use equality (==) with known CourseTag instances, " +
            "or filter client-side with AsEnumerable() if you need string operations.");
    }
}
