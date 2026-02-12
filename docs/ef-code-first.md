# Code-First Approach Study Guide

**Branch:** `ef/code-first`

## Overview
This branch demonstrates Entity Framework Core's Code-First approach focusing on:
- Creating database schema from C# classes
- Using migrations to evolve database schema
- Data seeding
- Fluent API configuration
- Managing database changes over time

## Project: Metigato Academy (10-Code-first)

A learning management system demonstrating code-first migrations and progressive schema evolution.

### Domain Entities

#### Course Entity (`Entites/Course.cs`)
```csharp
public class Course
{
    public int Id { get; set; }
    public required string CourseName { get; set; }
    public decimal Price { get; set; }
}
```

#### Instructor Entity (`Entites/Instructor.cs`)
```csharp
public class Instructor
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? OfficeId { get; set; }
}
```

#### Office Entity (`Entites/Office.cs`)
```csharp
public class Office
{
    public int Id { get; set; }
    public required string officeName { get; set; }
    public Instructor? instructor { get; set; }
}
```

### DbContext Configuration

**AppDbContext** (`Data/AppDbContext.cs`)
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Office> Offices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

        var constr = configuration.GetConnectionString("MetigatorCFM");

        optionsBuilder.UseSqlServer(constr)
            .LogTo(Console.WriteLine, LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

**Key Learnings:**
- Connection string management via `appsettings.json`
- Centralized configuration loading using `ConfigurationBuilder`
- SQL query logging for debugging

### Entity Configurations

#### CourseConfiguration (`Data/Config/CourseConfiguration.cs`)

```csharp
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(x => x.Id);

        // Don't auto-generate ID (manual control)
        builder.Property(x => x.Id).ValueGeneratedNever();

        // Configure string property
        builder.Property(x => x.CourseName)
            .HasColumnType("VARCHAR")  // SQL Server type
            .HasMaxLength(255)
            .IsRequired();

        // Configure decimal with precision
        builder.Property(x => x.Price)
            .HasPrecision(15, 2);  // 15 total digits, 2 after decimal

        builder.ToTable("Courses");

        // Seed data
        builder.HasData(LoadCourses());
    }

    private List<Course> LoadCourses() => new()
    {
        new Course { Id = 1, CourseName = "Mathmatics", Price = 1000m},
        new Course { Id = 2, CourseName = "Physics", Price = 2000m},
        new Course { Id = 3, CourseName = "Chemistry", Price = 1500m },
        new Course { Id = 4, CourseName = "Biology", Price = 1200m },
        new Course { Id = 5, CourseName = "CS-50", Price = 3000m },
    };
}
```

**Key Learnings:**
- `ValueGeneratedNever()`: Disables auto-increment for manual ID control
- `HasColumnType()`: Specifies exact SQL Server data type (VARCHAR vs NVARCHAR)
- `HasPrecision(15, 2)`: Controls decimal precision for monetary values
- `HasData()`: Seeds initial data that gets included in migrations

#### InstructorConfiguration

Similar pattern with data seeding for instructors.

#### OfficeConfiguration

Configures office entity with its properties.

## Migration History

This project demonstrates progressive schema evolution through migrations:

### Migration 1: Initial (`20251207105502_Initial.cs`)

**Created:**
- `Courses` table with Id, CourseName, Price
- `Instructors` table with Id, Name
- Seeded 5 courses
- Seeded 5 instructors

**Key Points:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Courses",
        columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false),
            CourseName = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false),
            Price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Courses", x => x.Id);
        });

    // Insert seed data
    migrationBuilder.InsertData(
        table: "Courses",
        columns: new[] { "Id", "CourseName", "Price" },
        values: new object[,]
        {
            { 1, "Mathmatics", 1000m },
            { 2, "Physics", 2000m },
            // ...
        });
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "Courses");
    migrationBuilder.DropTable(name: "Instructors");
}
```

**Learning Points:**
- `Up()`: Applies the migration (forward)
- `Down()`: Reverts the migration (rollback)
- Seed data is inserted via migrations
- Primary keys are automatically named `PK_{TableName}`

### Migration 2: Separate Name (`20251207120138_Separate Name.cs`)

**Changes:**
- Dropped `Name` column from Instructors table
- Added `FirstName` column (VARCHAR(50))
- Added `LastName` column (VARCHAR(50))
- Updated existing seed data to match new structure

**Key Points:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Remove old column
    migrationBuilder.DropColumn(
        name: "Name",
        table: "Instructors");

    // Add new columns
    migrationBuilder.AddColumn<string>(
        name: "FirstName",
        table: "Instructors",
        type: "VARCHAR(50)",
        maxLength: 50,
        nullable: false,
        defaultValue: "");

    migrationBuilder.AddColumn<string>(
        name: "LastName",
        table: "Instructors",
        type: "VARCHAR(50)",
        maxLength: 50,
        nullable: false,
        defaultValue: "");

    // Update existing data
    migrationBuilder.UpdateData(
        table: "Instructors",
        keyColumn: "Id",
        keyValue: 1,
        columns: new[] { "FirstName", "LastName" },
        values: new object[] { "Ahmed", "Abdullah" });
}
```

**Learning Points:**
- Migrations can modify existing schema
- `defaultValue: ""` prevents errors when adding non-nullable columns to existing data
- `UpdateData()` updates seed data in migrations
- Column renames require drop + add (no direct rename)

### Migration 3: Add Office Entity (`20251208024906_add-office-entity.cs`)

**Changes:**
- Created `Offices` table
- Added relationship between Office and Instructor
- Seeded office data

**Learning Points:**
- New entities can be added via migrations
- EF Core automatically creates foreign keys for relationships
- Migrations are timestamped (YYYYMMDDHHMMSS format)

## Code-First Workflow

### 1. Define/Modify Entity Classes
```csharp
public class NewEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### 2. Create Configuration (Optional but Recommended)
```csharp
public class NewEntityConfiguration : IEntityTypeConfiguration<NewEntity>
{
    public void Configure(EntityTypeBuilder<NewEntity> builder)
    {
        // Configure entity
    }
}
```

### 3. Add to DbContext
```csharp
public DbSet<NewEntity> NewEntities { get; set; }
```

### 4. Create Migration

**Option A: .NET CLI**
```bash
dotnet ef migrations add MigrationName
```

**Option B: Package Manager Console (Visual Studio)**
```powershell
Add-Migration MigrationName
```

### 5. Review Generated Migration
- Check `Up()` and `Down()` methods
- Verify SQL being generated
- Ensure data changes are handled correctly

### 6. Apply Migration

**Option A: .NET CLI**
```bash
dotnet ef database update
```

**Option B: Package Manager Console**
```powershell
Update-Database
```

### 7. Rollback (if needed)

**Option A: .NET CLI**
```bash
dotnet ef database update PreviousMigrationName
```

**Option B: Package Manager Console**
```powershell
Update-Database -Migration PreviousMigrationName
```

### 8. Remove Last Migration (if not applied)

**Option A: .NET CLI**
```bash
dotnet ef migrations remove
```

**Option B: Package Manager Console**
```powershell
Remove-Migration
```

## Key Concepts Covered

### 1. Migrations
- **Purpose**: Version control for database schema
- **Timestamp**: Ensures correct ordering
- **Up/Down**: Forward and backward compatibility
- **ModelSnapshot**: Tracks current model state

### 2. Data Seeding
- `HasData()` in entity configuration
- Seed data included in migrations
- `InsertData()` / `UpdateData()` in migrations
- Useful for lookup tables and test data

### 3. Value Generation
- `ValueGeneratedNever()`: Manual ID assignment
- `ValueGeneratedOnAdd()`: Auto-increment (default for integers)
- `ValueGeneratedOnAddOrUpdate()`: Computed columns

### 4. Column Configuration
- `HasColumnType()`: SQL-specific types
- `HasMaxLength()`: String length constraints
- `HasPrecision()`: Decimal precision
- `IsRequired()`: NOT NULL constraint

### 5. Table Configuration
- `ToTable()`: Specify table name
- Prevents EF from pluralizing names

## Best Practices Demonstrated

1. **Separate configurations** from entities using `IEntityTypeConfiguration<T>`
2. **Use `ApplyConfigurationsFromAssembly()`** instead of manual registration
3. **Version control migrations** - commit them with code
4. **Review migrations** before applying to production
5. **Include seed data** for reference/lookup tables
6. **Use meaningful migration names** that describe the change
7. **Test Down() methods** to ensure rollback works
8. **Use `appsettings.json`** for connection strings

## Common Pitfalls to Avoid

1. Modifying applied migrations (always create new one)
2. Forgetting to add migration after model changes
3. Not reviewing generated SQL
4. Deploying without testing migrations on copy of production
5. Using auto-generated IDs with HasData (use explicit IDs)

## Practical Exercises

1. Add a `Student` entity with FirstName, LastName, EnrollmentDate
2. Create a migration that adds Student table
3. Seed 10 students
4. Modify Student to split address into Street, City, ZipCode
5. Create migration to restructure existing data
6. Add an Enrollment entity linking Students to Courses
7. Practice rolling back and reapplying migrations

## Migration Commands Reference

### Essential Commands

| Task | .NET CLI | Package Manager Console |
|------|----------|------------------------|
| **Create Migration** | `dotnet ef migrations add MigrationName` | `Add-Migration MigrationName` |
| **Apply Migrations** | `dotnet ef database update` | `Update-Database` |
| **Rollback to Specific** | `dotnet ef database update TargetMigration` | `Update-Database -Migration TargetMigration` |
| **Remove Last Migration** | `dotnet ef migrations remove` | `Remove-Migration` |
| **List Migrations** | `dotnet ef migrations list` | `Get-Migration` |
| **Generate SQL Script** | `dotnet ef migrations script` | `Script-Migration` |
| **Drop Database** | `dotnet ef database drop` | `Drop-Database` |
| **Update to Specific** | `dotnet ef database update MigrationName` | `Update-Database -Migration MigrationName` |
| **Rollback All** | `dotnet ef database update 0` | `Update-Database -Migration 0` |

### Advanced Usage

**Generate SQL script for specific migration range:**
```bash
# .NET CLI
dotnet ef migrations script FromMigration ToMigration

# Package Manager Console
Script-Migration -From FromMigration -To ToMigration
```

**Generate idempotent SQL script (safe to run multiple times):**
```bash
# .NET CLI
dotnet ef migrations script --idempotent

# Package Manager Console
Script-Migration -Idempotent
```

**Apply migrations to a specific context (if multiple DbContexts):**
```bash
# .NET CLI
dotnet ef database update --context AppDbContext

# Package Manager Console
Update-Database -Context AppDbContext
```

**Create migration with different output directory:**
```bash
# .NET CLI
dotnet ef migrations add MigrationName --output-dir Data/Migrations

# Package Manager Console
Add-Migration MigrationName -OutputDir Data/Migrations
```

**View migration details:**
```bash
# .NET CLI
dotnet ef migrations list --verbose

# Package Manager Console
Get-Migration -Verbose
```

### Prerequisites

**For .NET CLI:**
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef
```

**For Package Manager Console:**
```powershell
# Install in your project
Install-Package Microsoft.EntityFrameworkCore.Tools
```

### Common Workflows

**Development Workflow:**
1. Modify entity models
2. `Add-Migration DescriptiveName`
3. Review generated migration
4. `Update-Database`
5. Test changes

**Production Deployment:**
1. Generate SQL script: `Script-Migration -Idempotent`
2. Review SQL carefully
3. Test on staging database
4. Apply to production during deployment window
5. Never use `Update-Database` directly on production

**Rollback Scenario:**
```powershell
# View migrations
Get-Migration

# Rollback to specific migration
Update-Database -Migration PreviousMigrationName

# Remove the bad migration file (if not yet pushed)
Remove-Migration
```

## Comparison: Code-First vs Database-First

**Code-First (this branch):**
- ✅ Version control for schema
- ✅ Works well with team collaboration
- ✅ Easy to reset development database
- ✅ Migrations provide audit trail
- ❌ Initial setup overhead

**Database-First:**
- ✅ Quick for existing databases
- ✅ Familiar to DBAs
- ❌ Schema changes require regeneration
- ❌ Harder to track history
