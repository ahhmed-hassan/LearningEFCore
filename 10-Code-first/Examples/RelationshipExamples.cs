using _10_Code_first.Data;
using _10_Code_first.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace _10_Code_first.Examples;

/// <summary>
/// Demonstrates the consequences of relationship configurations in this branch:
/// - Shadow navigation (no Course/Instructor property on Section)
/// - Required relationship (Course → Section) with Cascade delete
/// - Optional relationship (Instructor → Section) with SetNull
/// </summary>
public static class RelationshipExamples
{
    /// <summary>
    /// Demonstrates that you CANNOT navigate from Section to Course
    /// because we used shadow navigation (no Course property on Section)
    /// </summary>
    public static async Task Example1_ShadowNavigation_CannotNavigateToParent(ILogger logger)
    {
        logger.LogInformation("\n=== Example 1: Shadow Navigation - Cannot Navigate to Parent ===");

        using var context = new AppDbContext();

        var section = await context.Set<Section>().FirstAsync();

        // ❌ This would NOT compile - Section doesn't have Course property
        // var courseName = section.Course.CourseName;

        // ✅ Instead, we must query using the foreign key
        var course = await context.Courses.FirstAsync(c => c.Id == section.CourseId);
        logger.LogInformation("Section '{SectionName}' belongs to course: {CourseName}", section.SectionName, course.CourseName);

        // ✅ Or use a join
        var sectionWithCourse = await context.Set<Section>()
            .Where(s => s.Id == section.Id)
            .Select(s => new
            {
                SectionName = s.SectionName,
                CourseName = context.Courses.First(c => c.Id == s.CourseId).CourseName
            })
            .FirstAsync();

        logger.LogInformation("Via join: Section '{SectionName}' → Course '{CourseName}'", sectionWithCourse.SectionName, sectionWithCourse.CourseName);
    }

    /// <summary>
    /// Demonstrates querying from parent (Course) to children (Sections)
    /// This WORKS because Course has the Sections collection navigation property
    /// </summary>
    public static async Task Example2_ParentToChild_NavigationWorks(ILogger logger)
    {
        logger.LogInformation("\n=== Example 2: Parent → Child Navigation Works ===");

        using var context = new AppDbContext();

        // ✅ This works - Course has Sections property
        var course = await context.Courses
            .Include(c => c.Sections)
            .FirstAsync(c => c.Id == 1);

        logger.LogInformation($"Course: {course.CourseName}");
        logger.LogInformation($"Number of sections: {course.Sections.Count}");

        foreach (var section in course.Sections)
        {
            logger.LogInformation($"  - {section.SectionName}");
        }
    }

    /// <summary>
    /// Demonstrates REQUIRED relationship (Course → Section) with CASCADE delete
    /// When Course is deleted, all its Sections are automatically deleted
    /// </summary>
    public static async Task Example3_RequiredRelationship_CascadeDelete(ILogger logger)
    {
        logger.LogInformation("\n=== Example 3: Required Relationship - Cascade Delete ===");

        using var context = new AppDbContext();

        // Create a test course with sections
        var testCourse = new Course
        {
            Id = 999,
            CourseName = "Test Course",
            Price = 100m, Tag = Entites.ValueObjects.CourseTag.Advanced
        };
        context.Courses.Add(testCourse);
        await context.SaveChangesAsync();

        var testSections = new[]
        {
            new Section { Id = 9991, SectionName = "Test_S1", CourseId = 999, InstructorId = 1 },
            new Section { Id = 9992, SectionName = "Test_S2", CourseId = 999, InstructorId = 2 }
        };
        context.Set<Section>().AddRange(testSections);
        await context.SaveChangesAsync();

        logger.LogInformation($"Created course '{testCourse.CourseName}' with 2 sections");

        // Count sections before delete
        var sectionsBeforeDelete = await context.Set<Section>()
            .CountAsync(s => s.CourseId == 999);
        logger.LogInformation($"Sections before delete: {sectionsBeforeDelete}");

        // Delete the course
        context.Courses.Remove(testCourse);
        await context.SaveChangesAsync();

        logger.LogInformation($"Deleted course '{testCourse.CourseName}'");

        // Count sections after delete - should be 0 (CASCADE)
        var sectionsAfterDelete = await context.Set<Section>()
            .CountAsync(s => s.CourseId == 999);
        logger.LogInformation($"Sections after delete: {sectionsAfterDelete} (CASCADE deleted)");
    }

    /// <summary>
    /// Demonstrates that you CANNOT create a Section without a CourseId
    /// because it's a required relationship (non-nullable foreign key)
    /// </summary>
    public static async Task Example4_RequiredRelationship_CannotCreateWithoutParent(ILogger logger)
    {
        logger.LogInformation("\n=== Example 4: Required Relationship - Cannot Create Without Parent ===");

        using var context = new AppDbContext();

        try
        {
            // ❌ This will fail - CourseId is required (non-nullable)
            var orphanSection = new Section
            {
                Id = 9999,
                SectionName = "Orphan Section"
                // Missing CourseId!
            };

            // This would not even compile because CourseId has no default
            // But if we try to save it...
            // context.Set<Section>().Add(orphanSection);
            // await context.SaveChangesAsync();

            logger.LogInformation("❌ Cannot create section without CourseId (won't compile)");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Error: {ex.Message}");
        }

        // ✅ Correct way - must specify CourseId
        var validSection = new Section
        {
            Id = 9998,
            SectionName = "Valid Section",
            CourseId = 1  // Required!
        };
        context.Set<Section>().Add(validSection);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Successfully created section with CourseId: {validSection.SectionName}");

        // Cleanup
        context.Set<Section>().Remove(validSection);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Demonstrates OPTIONAL relationship (Instructor → Section) with SET NULL
    /// When Instructor is deleted, InstructorId is set to NULL (sections survive)
    /// </summary>
    public static async Task Example5_OptionalRelationship_SetNull(ILogger logger)
    {
        logger.LogInformation("\n=== Example 5: Optional Relationship - Set NULL on Delete ===");

        using var context = new AppDbContext();

        // Create a test instructor
        var testInstructor = new Instructor
        {
            Id = 999,
            FirstName = "Test",
            LastName = "Instructor"
        };
        context.Instructors.Add(testInstructor);
        await context.SaveChangesAsync();

        // Assign this instructor to some sections
        var sectionsToAssign = await context.Set<Section>()
            .Where(s => s.Id == 1 || s.Id == 2)
            .ToListAsync();

        foreach (var section in sectionsToAssign)
        {
            section.InstructorId = 999;
        }
        await context.SaveChangesAsync();

        logger.LogInformation($"Assigned instructor '{testInstructor.FirstName} {testInstructor.LastName}' to {sectionsToAssign.Count} sections");

        // Count sections assigned to this instructor
        var sectionsBeforeDelete = await context.Set<Section>()
            .CountAsync(s => s.InstructorId == 999);
        logger.LogInformation($"Sections assigned to instructor: {sectionsBeforeDelete}");

        // Delete the instructor
        context.Instructors.Remove(testInstructor);
        await context.SaveChangesAsync();

        logger.LogInformation($"Deleted instructor '{testInstructor.FirstName} {testInstructor.LastName}'");

        // Check if sections still exist (they should)
        var sectionsAfterDelete = await context.Set<Section>()
            .Where(s => s.Id == 1 || s.Id == 2)
            .ToListAsync();

        logger.LogInformation($"Sections after instructor delete: {sectionsAfterDelete.Count} (still exist)");

        foreach (var section in sectionsAfterDelete)
        {
            logger.LogInformation($"  - {section.SectionName}: InstructorId = {section.InstructorId?.ToString() ?? "NULL (SetNull worked!)"}");
        }
    }

    /// <summary>
    /// Demonstrates creating a Section with optional instructor
    /// Shows that InstructorId can be NULL but CourseId cannot
    /// </summary>
    public static async Task Example6_OptionalVsRequired_CreatingEntities(ILogger logger)
    {
        logger.LogInformation("\n=== Example 6: Optional vs Required When Creating ===");

        using var context = new AppDbContext();

        // ✅ Can create section without instructor (optional relationship)
        var sectionWithoutInstructor = new Section
        {
            Id = 9997,
            SectionName = "Unassigned Section",
            CourseId = 1,           // ✅ Required - must specify
            InstructorId = null     // ✅ Optional - can be NULL
        };

        context.Set<Section>().Add(sectionWithoutInstructor);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Created section without instructor: {sectionWithoutInstructor.SectionName}");
        logger.LogInformation($"   CourseId: {sectionWithoutInstructor.CourseId} (required)");
        logger.LogInformation($"   InstructorId: {sectionWithoutInstructor.InstructorId?.ToString() ?? "NULL"} (optional)");

        // Later, assign an instructor
        sectionWithoutInstructor.InstructorId = 1;
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Later assigned instructor: InstructorId = {sectionWithoutInstructor.InstructorId}");

        // Cleanup
        context.Set<Section>().Remove(sectionWithoutInstructor);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Demonstrates querying patterns when you don't have child-to-parent navigation
    /// </summary>
    public static async Task Example7_QueryingWithoutNavigation(ILogger logger)
    {
        logger.LogInformation("\n=== Example 7: Querying Without Navigation Properties ===");

        using var context = new AppDbContext();

        // Get sections for a specific course (using FK)
        var mathSections = await context.Set<Section>()
            .Where(s => s.CourseId == 1)
            .ToListAsync();

        logger.LogInformation($"Sections for Course 1: {mathSections.Count}");

        // Get sections taught by a specific instructor (using FK)
        var instructor1Sections = await context.Set<Section>()
            .Where(s => s.InstructorId == 1)
            .ToListAsync();

        logger.LogInformation($"Sections taught by Instructor 1: {instructor1Sections.Count}");

        // Get unassigned sections
        var unassignedSections = await context.Set<Section>()
            .Where(s => s.InstructorId == null)
            .ToListAsync();

        logger.LogInformation($"Unassigned sections: {unassignedSections.Count}");

        // Complex query: Get section details with course and instructor names
        var sectionDetails = await context.Set<Section>()
            .Select(s => new
            {
                SectionName = s.SectionName,
                CourseName = context.Courses
                    .Where(c => c.Id == s.CourseId)
                    .Select(c => c.CourseName)
                    .First(),
                InstructorName = s.InstructorId.HasValue
                    ? context.Instructors
                        .Where(i => i.Id == s.InstructorId)
                        .Select(i => i.FirstName + " " + i.LastName)
                        .First()
                    : "Unassigned"
            })
            .Take(3)
            .ToListAsync();

        logger.LogInformation("\nSection Details (first 3):");
        foreach (var detail in sectionDetails)
        {
            logger.LogInformation($"  {detail.SectionName} | {detail.CourseName} | {detail.InstructorName}");
        }
    }

    /// <summary>
    /// Demonstrates reassigning sections between instructors
    /// </summary>
    public static async Task Example8_ReassigningRelationships(ILogger logger)
    {
        logger.LogInformation("\n=== Example 8: Reassigning Relationships ===");

        using var context = new AppDbContext();

        // Get a section currently assigned to instructor 1
        var section = await context.Set<Section>()
            .FirstOrDefaultAsync(s => s.InstructorId == 1);

        if (section != null)
        {
            var oldInstructorId = section.InstructorId;
            logger.LogInformation($"Section '{section.SectionName}' is currently taught by Instructor {oldInstructorId}");

            // Reassign to instructor 2
            section.InstructorId = 2;
            await context.SaveChangesAsync();

            logger.LogInformation($"Reassigned to Instructor {section.InstructorId}");

            // Revert for consistency
            section.InstructorId = oldInstructorId;
            await context.SaveChangesAsync();
            logger.LogInformation($"Reverted back to Instructor {oldInstructorId}");
        }
    }
}
