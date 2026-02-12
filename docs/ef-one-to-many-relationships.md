# One-to-Many Relationships Study Guide

**Branch:** `ef/one-to-many`

## Overview
This branch demonstrates Entity Framework Core's one-to-many relationship patterns focusing on:
- Required vs optional relationships
- Configuring relationships from the parent entity
- Navigation properties (collection and reference)
- Foreign key configuration
- Cascade delete behavior
- Data seeding with relationships

## Project: Metigato Academy Extended (10-Code-first)

Building on the code-first foundation, this branch adds the `Section` entity to demonstrate:
- **Course → Sections**: Required one-to-many (a section MUST belong to a course)
- **Instructor → Sections**: Optional one-to-many (a section MAY have an instructor)

### Domain Entities

#### Course Entity (`Entites/Course.cs`)
```csharp
public class Course
{
    public int Id { get; set; }
    public required string CourseName { get; set; }
    public decimal Price { get; set; }

    // Collection navigation property
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
```

**Key Learning:**
- `ICollection<Section>` represents the "many" side of one-to-many
- Initialized with empty collection to prevent null reference errors
- Using `ICollection<T>` is preferred over `List<T>` for flexibility

#### Instructor Entity (`Entites/Instructor.cs`)
```csharp
public class Instructor
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? OfficeId { get; set; }

    // Collection navigation property
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
```

**Key Learning:**
- Similar pattern as Course
- Nullable reference allows instructor to be assigned later

#### Section Entity (`Entites/Section.cs`)
```csharp
public class Section
{
    public int Id { get; set; }
    public required string SectionName { get; set; }

    // Required foreign key - Section MUST have a Course
    public int CourseId { get; set; }

    // Optional foreign key - Section MAY have an Instructor
    public int? InstructorId { get; set; }
}
```

**Key Learning:**
- `CourseId` is **non-nullable** (int) = **required relationship**
- `InstructorId` is **nullable** (int?) = **optional relationship**
- Foreign key properties make relationships explicit
- No navigation properties defined (configured via Fluent API)

### Relationship Configurations

#### CourseConfiguration (`Data/Config/CourseConfiguration.cs`)

```csharp
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CourseName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(15, 2);

        builder.ToTable("Courses");

        // Configure one-to-many relationship with Section
        builder.HasMany(c => c.Sections)
            .WithOne()  // Section doesn't have Course navigation property
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(LoadCourses());
    }
}
```

**Key Learnings:**
- `HasMany(c => c.Sections)`: Course has many sections
- `WithOne()`: Each section belongs to one course (no navigation property needed)
- `HasForeignKey(s => s.CourseId)`: Explicitly specify FK column
- `OnDelete(DeleteBehavior.Cascade)`: Delete all sections when course is deleted
- Relationship is **required** because `CourseId` is non-nullable

**Why configure from Course (parent) side?**
- Parent entity "owns" the relationship semantically
- More intuitive: "A course has many sections"
- Centralizes relationship logic

#### InstructorConfiguration (`Data/Config/InstructorConfiguration.cs`)

```csharp
public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.FirstName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(50);

        builder.ToTable("Instructors");

        // Configure one-to-many relationship with Section
        builder.HasMany(i => i.Sections)
            .WithOne()
            .HasForeignKey(s => s.InstructorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasData(LoadInstructors());
    }
}
```

**Key Learnings:**
- `OnDelete(DeleteBehavior.SetNull)`: When instructor is deleted, `InstructorId` becomes NULL
- Relationship is **optional** because `InstructorId` is nullable
- Preserves section history even if instructor leaves

#### SectionConfiguration (`Data/Config/SectionConfiguration.cs`)

```csharp
internal class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.SectionName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(255)
            .IsRequired();

        builder.ToTable("Sections");

        builder.HasData(LoadSections());
    }

    private static List<Section> LoadSections() => new()
    {
        new Section { Id = 1, SectionName = "S_MA1", CourseId = 1, InstructorId = 1},
        new Section { Id = 2, SectionName = "S_MA2", CourseId = 1, InstructorId = 2},
        new Section { Id = 3, SectionName = "S_PH1", CourseId = 2, InstructorId = 1},
        new Section { Id = 4, SectionName = "S_PH2", CourseId = 2, InstructorId = 3},
        new Section { Id = 5, SectionName = "S_CH1", CourseId = 3, InstructorId = 2},
        new Section { Id = 6, SectionName = "S_CH2", CourseId = 3, InstructorId = 3},
        new Section { Id = 7, SectionName = "S_BI1", CourseId = 4, InstructorId = 4},
        new Section { Id = 8, SectionName = "S_BI2", CourseId = 4, InstructorId = 5},
        new Section { Id = 9, SectionName = "S_CS1", CourseId = 5, InstructorId = 4},
        new Section { Id = 10, SectionName = "S_CS2", CourseId = 5, InstructorId = 5},
        new Section { Id = 11, SectionName = "S_CS3", CourseId = 5, InstructorId = 4}
    };
}
```

**Key Learnings:**
- Relationship configuration is in **parent entities** (Course, Instructor)
- Section configuration focuses on its own properties
- Seed data demonstrates the relationships:
  - Course 1 (Mathematics) has 2 sections
  - Course 5 (CS-50) has 3 sections
  - Instructor 4 teaches 3 sections

## One-to-Many Relationship Patterns

### Pattern 1: Required Relationship (Course → Section)

**Characteristics:**
- Child MUST have a parent
- Non-nullable foreign key (`int CourseId`)
- Cascade delete (delete children with parent)

**Use Cases:**
- Order → OrderLines
- Blog Post → Comments (if comments don't exist independently)
- Invoice → InvoiceItems

**Configuration:**
```csharp
// In parent configuration
builder.HasMany(parent => parent.Children)
    .WithOne()
    .HasForeignKey(child => child.ParentId)
    .OnDelete(DeleteBehavior.Cascade);  // Delete children with parent
```

**Database Schema:**
```sql
CREATE TABLE Sections (
    Id INT PRIMARY KEY,
    SectionName VARCHAR(255) NOT NULL,
    CourseId INT NOT NULL,  -- Non-nullable FK
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
);
```

### Pattern 2: Optional Relationship (Instructor → Section)

**Characteristics:**
- Child MAY have a parent
- Nullable foreign key (`int? InstructorId`)
- SetNull on delete (preserve child, clear reference)

**Use Cases:**
- Manager → Employees (employee can exist without manager)
- Category → Products (product can be uncategorized)
- Author → Books (book can have unknown/deleted author)

**Configuration:**
```csharp
// In parent configuration
builder.HasMany(parent => parent.Children)
    .WithOne()
    .HasForeignKey(child => child.ParentId)
    .OnDelete(DeleteBehavior.SetNull);  // Keep child, set FK to NULL
```

**Database Schema:**
```sql
CREATE TABLE Sections (
    Id INT PRIMARY KEY,
    SectionName VARCHAR(255) NOT NULL,
    InstructorId INT NULL,  -- Nullable FK
    FOREIGN KEY (InstructorId) REFERENCES Instructors(Id) ON DELETE SET NULL
);
```

## Delete Behaviors Explained

### Cascade (Required Relationships)
```csharp
.OnDelete(DeleteBehavior.Cascade)
```
- **Effect**: Delete all child records when parent is deleted
- **Use When**: Children cannot exist without parent
- **Example**: Delete all sections when course is deleted

### SetNull (Optional Relationships)
```csharp
.OnDelete(DeleteBehavior.SetNull)
```
- **Effect**: Set foreign key to NULL when parent is deleted
- **Use When**: Children should survive parent deletion
- **Example**: Section continues without instructor

### Restrict
```csharp
.OnDelete(DeleteBehavior.Restrict)
```
- **Effect**: Prevent parent deletion if children exist
- **Use When**: Need manual cleanup before deletion
- **Example**: Cannot delete category if products exist

### NoAction
```csharp
.OnDelete(DeleteBehavior.NoAction)
```
- **Effect**: No automatic action (application handles it)
- **Use When**: Complex business logic required
- **Note**: Different from Restrict in some databases

## Navigation Properties: Full vs Shadow

### Full Navigation (Both Sides)
```csharp
// Parent
public class Course
{
    public ICollection<Section> Sections { get; set; }
}

// Child
public class Section
{
    public int CourseId { get; set; }
    public Course Course { get; set; }  // Reference navigation
}

// Configuration
builder.HasMany(c => c.Sections)
    .WithOne(s => s.Course)  // Both sides specified
    .HasForeignKey(s => s.CourseId);
```

**Pros:**
- Easier to navigate relationships in code
- More intuitive for complex queries

**Cons:**
- More properties to maintain
- Risk of circular references in serialization

### Shadow Navigation (One Side Only)
```csharp
// Parent
public class Course
{
    public ICollection<Section> Sections { get; set; }
}

// Child
public class Section
{
    public int CourseId { get; set; }
    // No Course navigation property
}

// Configuration
builder.HasMany(c => c.Sections)
    .WithOne()  // No navigation property on child
    .HasForeignKey(s => s.CourseId);
```

**Pros:**
- Simpler entities
- No circular reference issues
- Still fully functional

**Cons:**
- Cannot navigate from child to parent easily
- Need to use queries: `context.Courses.First(c => c.Id == section.CourseId)`

**This Branch Uses**: Shadow navigation (WithOne() without parameter)

## Querying One-to-Many Relationships

### Eager Loading (Include)
```csharp
// Load course with all its sections
var course = await context.Courses
    .Include(c => c.Sections)
    .FirstOrDefaultAsync(c => c.Id == 1);

// Access sections
foreach (var section in course.Sections)
{
    Console.WriteLine(section.SectionName);
}
```

### Explicit Loading
```csharp
// Load course first
var course = await context.Courses.FindAsync(1);

// Load sections later
await context.Entry(course)
    .Collection(c => c.Sections)
    .LoadAsync();
```

### Filtering Related Data
```csharp
// Load course with sections taught by specific instructor
var course = await context.Courses
    .Include(c => c.Sections.Where(s => s.InstructorId == 1))
    .FirstOrDefaultAsync(c => c.Id == 1);
```

### Projection (Select Only What You Need)
```csharp
var courseInfo = await context.Courses
    .Where(c => c.Id == 1)
    .Select(c => new
    {
        CourseName = c.CourseName,
        SectionCount = c.Sections.Count,
        Sections = c.Sections.Select(s => s.SectionName).ToList()
    })
    .FirstOrDefaultAsync();
```

## Adding/Removing Related Entities

### Add Section to Course
```csharp
// Option 1: Add to collection
var course = await context.Courses
    .Include(c => c.Sections)
    .FirstAsync(c => c.Id == 1);

course.Sections.Add(new Section
{
    Id = 12,
    SectionName = "S_MA3",
    CourseId = 1,  // Must set FK
    InstructorId = 2
});

await context.SaveChangesAsync();

// Option 2: Set FK directly
var newSection = new Section
{
    Id = 12,
    SectionName = "S_MA3",
    CourseId = 1,
    InstructorId = 2
};

context.Sections.Add(newSection);
await context.SaveChangesAsync();
```

### Remove Section from Course
```csharp
var course = await context.Courses
    .Include(c => c.Sections)
    .FirstAsync(c => c.Id == 1);

var sectionToRemove = course.Sections.First(s => s.Id == 12);
course.Sections.Remove(sectionToRemove);

// OR
context.Sections.Remove(sectionToRemove);

await context.SaveChangesAsync();
```

### Reassign Section to Different Course
```csharp
var section = await context.Sections.FindAsync(12);
section.CourseId = 2;  // Move to different course
await context.SaveChangesAsync();
```

## Migration for One-to-Many

The migration that added the Section entity:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Sections",
        columns: table => new
        {
            Id = table.Column<int>(nullable: false),
            SectionName = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false),
            CourseId = table.Column<int>(nullable: false),  // Required FK
            InstructorId = table.Column<int>(nullable: true) // Optional FK
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Sections", x => x.Id);

            // Foreign key to Courses with CASCADE
            table.ForeignKey(
                name: "FK_Sections_Courses_CourseId",
                column: x => x.CourseId,
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Foreign key to Instructors with SET NULL
            table.ForeignKey(
                name: "FK_Sections_Instructors_InstructorId",
                column: x => x.InstructorId,
                principalTable: "Instructors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        });

    // Create index on foreign keys for better query performance
    migrationBuilder.CreateIndex(
        name: "IX_Sections_CourseId",
        table: "Sections",
        column: "CourseId");

    migrationBuilder.CreateIndex(
        name: "IX_Sections_InstructorId",
        table: "Sections",
        column: "InstructorId");
}
```

**Key Points:**
- EF Core automatically creates indexes on foreign keys
- ON DELETE CASCADE vs ON DELETE SET NULL based on configuration
- Named constraints for better database documentation

## Best Practices

1. **Configure from Parent**: Define relationships in parent entity configuration
2. **Explicit Foreign Keys**: Include FK properties in child entities for clarity
3. **Initialize Collections**: `= new List<T>()` prevents null reference errors
4. **Choose Correct Delete Behavior**: Match business requirements
   - Cascade for dependent children
   - SetNull for independent children
   - Restrict when manual cleanup needed
5. **Use ICollection<T>**: More flexible than List<T>
6. **Consider Navigation Properties**: Full vs shadow based on needs
7. **Index Foreign Keys**: EF Core does this automatically

## Common Pitfalls

1. **Forgetting to Include**: Related data not loaded (N+1 query problem)
   ```csharp
   // Bad: Sections not loaded
   var course = context.Courses.First();
   var count = course.Sections.Count; // Throws exception or loads lazily

   // Good: Explicitly include
   var course = context.Courses.Include(c => c.Sections).First();
   ```

2. **Modifying Disconnected Entities**: Update conflicts
   ```csharp
   // Attach entity before modifying collections
   context.Attach(course);
   course.Sections.Add(newSection);
   ```

3. **Wrong Delete Behavior**: Unexpected cascades or constraint violations
4. **Not Setting Required FK**: Validation errors on save
5. **Circular References in JSON**: When serializing entities

## Practical Exercises

1. Query all sections for "Mathematics" course
2. Find all sections taught by a specific instructor
3. Count total sections per course
4. Create a new section and assign to course and instructor
5. Reassign all sections from one instructor to another
6. Delete a course and verify cascade delete works
7. Delete an instructor and verify sections remain with NULL instructor
8. Implement validation: course must have at least one section

## Real-World Scenarios

### Scenario 1: Unassign Instructor
```csharp
var section = await context.Sections.FindAsync(1);
section.InstructorId = null;  // Remove instructor assignment
await context.SaveChangesAsync();
```

### Scenario 2: Get Instructor's Workload
```csharp
var instructorWorkload = await context.Instructors
    .Select(i => new
    {
        Name = $"{i.FirstName} {i.LastName}",
        SectionCount = i.Sections.Count,
        Courses = i.Sections
            .Select(s => context.Courses.First(c => c.Id == s.CourseId).CourseName)
            .Distinct()
            .ToList()
    })
    .ToListAsync();
```

### Scenario 3: Find Unassigned Sections
```csharp
var unassignedSections = await context.Sections
    .Where(s => s.InstructorId == null)
    .ToListAsync();
```

### Scenario 4: Course with Most Sections
```csharp
var popularCourse = await context.Courses
    .OrderByDescending(c => c.Sections.Count)
    .Select(c => new
    {
        c.CourseName,
        SectionCount = c.Sections.Count
    })
    .FirstAsync();
```

## Summary

This branch demonstrates:
- ✅ Required one-to-many (Course → Sections)
- ✅ Optional one-to-many (Instructor → Sections)
- ✅ Cascade vs SetNull delete behaviors
- ✅ Shadow navigation properties
- ✅ Configuring relationships from parent entity
- ✅ Foreign key conventions
- ✅ Data seeding with relationships
- ✅ Querying related data

**Next Steps**: Study many-to-many relationships (Student ↔ Course enrollment)
