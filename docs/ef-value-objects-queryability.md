# Value Objects & Queryability Study Guide

**Branch:** `ef/value-objects`

## Table of Contents

- [Overview](#overview)
- [Key Question](#key-question)
- [1. Owned Entities — Address on Instructor](#1-owned-entities--address-on-instructor)
  - [Value Object](#value-object-entitesvalueobjectsaddresscs)
  - [Configuration](#configuration-dataconfiginstructorconfigurationcs)
  - [Seeding Owned Types](#seeding-owned-types)
  - [Queryability: Fully Server-Side](#queryability-fully-server-side)
- [2. Value Converters — CourseTag on Course](#2-value-converters--coursetag-on-course)
  - [Value Object](#value-object-entitesvalueobjectscoursetagcs)
  - [Configuration](#configuration-dataconfigcourseconfigurationcs)
  - [Queryability: Equality Works, Member Access Doesn't](#queryability-equality-works-member-access-doesnt)
  - [Note on operator ==](#note-on-operator-)
- [3. Computed Properties — DateRange on Section](#3-computed-properties--daterange-on-section-the-queryability-problem)
  - [Value Object](#value-object-entitesvalueobjectsdaterangecs)
  - [Configuration](#configuration-dataconfigsectionconfigurationcs)
  - [The Problem: Computed Properties Cannot Be Queried](#the-problem-computed-properties-cannot-be-queried)
  - [Alternative A: Rewrite with Persisted Properties](#alternative-a-rewrite-with-persisted-properties-recommended)
  - [Alternative B: HasComputedColumnSql](#alternative-b-hascomputedcolumnsql-explained)
  - [Alternative C: Client-Side Evaluation](#alternative-c-client-side-evaluation-last-resort)
- [Comparison Table](#comparison-table)
- [Rules of Thumb](#rules-of-thumb)
- [File Structure](#file-structure)
- [Migration Commands](#migration-commands)
- [Forward Note: EF Core 8+ ComplexProperty](#forward-note-ef-core-8-complexproperty)

## Overview

This branch explores how EF Core handles **value objects** (rich domain types) and critically examines **what can and cannot be translated to SQL** when querying. Three patterns are demonstrated, each with different queryability characteristics:

1. **Owned Entities (`OwnsOne`)** — value object stored as columns in the parent table
2. **Value Converters (`HasConversion`)** — value object stored as a single column
3. **Computed Properties** — C# properties derived from persisted data (the queryability problem)

## Key Question

> If a value object has properties or methods computed from persisted data, can LINQ queries that reference them be pushed to SQL Server? Or do they require client-side evaluation?

**Answer:** Only properties that map to actual database columns can be translated to SQL. Computed properties (even simple ones like `EndDate - StartDate`) cannot be translated and will throw `InvalidOperationException`. This guide demonstrates the problem and three workarounds.

---

## 1. Owned Entities — Address on Instructor

### Value Object (`Entites/ValueObjects/Address.cs`)

```csharp
public class Address
{
    public string Street { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string ZipCode { get; private set; } = null!;

    private Address() { }  // EF Core needs this

    public Address(string street, string city, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));
        // ... validation for city and zipCode
        Street = street;
        City = city;
        ZipCode = zipCode;
    }
}
```

**Key patterns:**
- **Private setters** — immutability enforced; only the constructor can set values
- **Private parameterless constructor** — required by EF Core to materialize from DB
- **`= null!;`** — suppresses CS8618 warning; EF populates via reflection, not the constructor
- **Constructor validation** — domain rules enforced at creation time

### Configuration (`Data/Config/InstructorConfiguration.cs`)

```csharp
builder.OwnsOne(i => i.Address, addressBuilder =>
{
    addressBuilder.Property(a => a.Street)
        .HasColumnName("Address_Street")
        .HasColumnType("VARCHAR")
        .HasMaxLength(200);

    addressBuilder.Property(a => a.City)
        .HasColumnName("Address_City")
        .HasColumnType("VARCHAR")
        .HasMaxLength(100);

    addressBuilder.Property(a => a.ZipCode)
        .HasColumnName("Address_ZipCode")
        .HasColumnType("VARCHAR")
        .HasMaxLength(10);
});
```

**What `OwnsOne` does:**
- Stores Address properties as **columns in the Instructors table** (NOT a separate table)
- Explicit `HasColumnName` controls the column naming (default would be `Address_Street` anyway, but explicit is clearer)
- The Address has no `Id` column — it's not an entity, it's part of the Instructor row

### Seeding Owned Types

```csharp
// Regular entity seed
builder.HasData(LoadInstructors());

// Owned type seed — requires anonymous objects with the owner's primary key
builder.OwnsOne(i => i.Address).HasData(
    new { InstructorId = 1, Street = "123 Main St", City = "Cairo", ZipCode = "11511" },
    new { InstructorId = 2, Street = "456 Elm Ave", City = "Alexandria", ZipCode = "21500" },
    new { InstructorId = 3, Street = "789 Oak Blvd", City = "Cairo", ZipCode = "11765" }
);
```

**Important:** EF Core requires the owner's PK (`InstructorId`) in the anonymous object to link the owned data to the parent entity. You cannot set Address directly in the `Instructor` object used in `HasData`.

### Queryability: Fully Server-Side

**Example 1 — Filter by owned property:**
```csharp
var cairoInstructors = await context.Instructors
    .Where(i => i.Address != null && i.Address.City == "Cairo")
    .ToListAsync();
```

Generated SQL:
```sql
SELECT [i].[Id], [i].[FirstName], [i].[LastName], [i].[OfficeId],
       [i].[Address_City], [i].[Address_Street], [i].[Address_ZipCode]
FROM [Instructors] AS [i]
WHERE ([i].[Address_City] IS NOT NULL) AND [i].[Address_City] = 'Cairo'
```

EF Core maps `i.Address.City` directly to the `Address_City` column. The WHERE runs entirely on SQL Server.

**Example 2 — No `.Include()` needed:**
```csharp
var instructor = await context.Instructors.FirstAsync(i => i.Id == 1);
// instructor.Address is already loaded — it's part of the same table row
```

Owned entities are always loaded with their owner. Since the Address columns live in the Instructors table, every SELECT automatically includes them.

### Migration

```
Add-Migration AddInstructorAddress
Update-Database
```

The migration adds three nullable VARCHAR columns to the existing Instructors table — no new table is created.

---

## 2. Value Converters — CourseTag on Course

### Value Object (`Entites/ValueObjects/CourseTag.cs`)

```csharp
public class CourseTag : IEquatable<CourseTag>
{
    public string Value { get; }

    public static readonly CourseTag Beginner = new("BEGINNER");
    public static readonly CourseTag Intermediate = new("INTERMEDIATE");
    public static readonly CourseTag Advanced = new("ADVANCED");

    public CourseTag(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tag value cannot be empty.", nameof(value));
        Value = value.ToUpperInvariant();
    }

    // Equality overrides: Equals, GetHashCode, operator ==, operator !=
}
```

**Key patterns:**
- **Static factory members** — well-known instances like `CourseTag.Advanced`
- **`ToUpperInvariant()`** — normalization ensures consistency
- **Equality by value** — two CourseTags with the same string are equal

### Configuration (`Data/Config/CourseConfiguration.cs`)

```csharp
builder.Property(x => x.Tag)
    .HasConversion(
        tag => tag.Value,            // C# -> DB: extract the string
        value => new CourseTag(value) // DB -> C#: reconstruct the object
    )
    .HasColumnType("VARCHAR")
    .HasMaxLength(50)
    .IsRequired();
```

**What `HasConversion` does:**
- Maps a complex C# type to a **single column** in the database
- Two lambdas: one for writing (C# -> DB), one for reading (DB -> C#)
- The column stores plain strings like `"ADVANCED"`, `"BEGINNER"`

### Queryability: Equality Works, Member Access Doesn't

**Example 3 — Equality translates to SQL:**
```csharp
var advancedCourses = await context.Courses
    .Where(c => c.Tag == CourseTag.Advanced)
    .ToListAsync();
```

Generated SQL:
```sql
SELECT [c].[Id], [c].[CourseName], [c].[Price], [c].[Tag]
FROM [Courses] AS [c]
WHERE [c].[Tag] = 'ADVANCED'
```

EF Core applies the converter to turn `CourseTag.Advanced` into `"ADVANCED"` in the WHERE clause.

**Example 4 — Accessing `.Value` fails:**
```csharp
// This FAILS — EF can't translate .Value.StartsWith()
var result = await context.Courses
    .Where(c => c.Tag.Value.StartsWith("ADV"))
    .ToListAsync();
// InvalidOperationException: could not be translated
```

EF Core treats Tag as a single opaque column. It knows how to compare the whole value (via the converter) but cannot drill into `.Value` to apply string methods.

### Note on `operator ==`

The `operator ==` override on CourseTag is **not needed for EF Core queries**. EF works with expression trees — when it sees `==` in LINQ, it uses the value converter regardless of the C# operator. The override is for regular C# code outside of LINQ (e.g., `if (course.Tag == CourseTag.Advanced)`), where without it `==` would do reference equality on a class.

### Migration

```
Add-Migration AddCourseTag
Update-Database
```

Adds a single `Tag VARCHAR(50) NOT NULL` column to the Courses table.

---

## 3. Computed Properties — DateRange on Section (The Queryability Problem)

This is the core concern: what happens when a value object has properties **computed from persisted data** that are NOT themselves stored in the database?

### Value Object (`Entites/ValueObjects/DateRange.cs`)

```csharp
public class DateRange
{
    // PERSISTED — these map to columns
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    // COMPUTED — NOT persisted, NOT mapped to columns
    public int TotalDays => (EndDate - StartDate).Days;
    public bool IsActive => StartDate <= DateTime.Now && DateTime.Now <= EndDate;

    private DateRange() { }

    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("EndDate must be after StartDate.");
        StartDate = startDate;
        EndDate = endDate;
    }

    // COMPUTED METHOD — also NOT translatable in LINQ
    public bool Contains(DateTime date) => StartDate <= date && date <= EndDate;
}
```

### Configuration (`Data/Config/SectionConfiguration.cs`)

```csharp
builder.OwnsOne(s => s.Schedule, scheduleBuilder =>
{
    scheduleBuilder.Property(d => d.StartDate)
        .HasColumnName("Schedule_StartDate")
        .IsRequired();

    scheduleBuilder.Property(d => d.EndDate)
        .HasColumnName("Schedule_EndDate")
        .IsRequired();

    // Explicitly ignore computed properties — EF must not try to map them
    scheduleBuilder.Ignore(d => d.TotalDays);
    scheduleBuilder.Ignore(d => d.IsActive);
});
```

**Critical:** Without `Ignore()`, EF Core would try to create columns for `TotalDays` and `IsActive`. Since they're expression-bodied properties with no setter, this would fail. `Ignore()` tells EF these are C#-only.

### The Problem: Computed Properties Cannot Be Queried

**Example 5 — All three fail:**

```csharp
// Attempt 1: computed property
await context.Sections.Where(s => s.Schedule.TotalDays > 60).ToListAsync();
// InvalidOperationException: Translation of member 'TotalDays' on entity type 'DateRange' failed.

// Attempt 2: method call
await context.Sections.Where(s => s.Schedule.Contains(today)).ToListAsync();
// InvalidOperationException: could not be translated

// Attempt 3: computed property referencing DateTime.Now
await context.Sections.Where(s => s.Schedule.IsActive).ToListAsync();
// InvalidOperationException: Translation of member 'IsActive' failed.
```

**Why:** EF Core builds an expression tree from LINQ and translates it to SQL. It can only translate things it knows how to map — persisted properties that correspond to columns. `TotalDays`, `Contains()`, and `IsActive` exist only in C#; there are no corresponding columns, so EF has nothing to translate them to.

### Alternative A: Rewrite with Persisted Properties (Recommended)

Instead of using the computed property, manually express the same logic using the persisted columns:

```csharp
// Instead of: s.Schedule.Contains(today)
var activeSections = await context.Sections
    .Where(s => s.Schedule.StartDate <= today && today <= s.Schedule.EndDate)
    .ToListAsync();
```

```sql
WHERE [s].[Schedule_StartDate] <= @__today_0 AND @__today_0 <= [s].[Schedule_EndDate]
```

```csharp
// Instead of: s.Schedule.TotalDays > 60
var longSections = await context.Sections
    .Where(s => EF.Functions.DateDiffDay(s.Schedule.StartDate, s.Schedule.EndDate) > 60)
    .ToListAsync();
```

```sql
WHERE DATEDIFF(day, [s].[Schedule_StartDate], [s].[Schedule_EndDate]) > 60
```

`EF.Functions.DateDiffDay()` is an EF Core extension that translates directly to SQL Server's `DATEDIFF`. Both queries run entirely server-side.

### Alternative B: HasComputedColumnSql (Explained)

Instead of ignoring the property, map it to a **SQL-computed column**:

```csharp
// Replace: scheduleBuilder.Ignore(d => d.TotalDays);
// With:
scheduleBuilder.Property(d => d.TotalDays)
    .HasColumnName("Schedule_TotalDays")
    .HasComputedColumnSql("DATEDIFF(DAY, Schedule_StartDate, Schedule_EndDate)");
```

This creates a real column in SQL Server that's automatically computed. EF Core can then query it:

```csharp
// This WOULD work with HasComputedColumnSql:
var longSections = await context.Sections
    .Where(s => s.Schedule.TotalDays > 60)
    .ToListAsync();
// SQL: WHERE [Schedule_TotalDays] > 60
```

**Why we didn't run it:** You can't both `Ignore()` and map a property in the same configuration. We chose `Ignore()` so Example 5 demonstrates the failure. To use `HasComputedColumnSql`, replace the `Ignore()` call and create a new migration.

**Trade-offs:**

| | Pros | Cons |
|---|---|---|
| **Computed Column** | Query translates to SQL; no LINQ rewrite needed; always up-to-date | Formula lives in C# AND SQL (must stay in sync); only works for same-row expressions |

**Limitation:** Cannot use for cross-table aggregations. For example, if `EndDate = StartDate + SUM(child.Duration)`, `HasComputedColumnSql` won't help — you'd need a VIEW, stored procedure, or a LINQ rewrite with explicit joins.

### Alternative C: Client-Side Evaluation (Last Resort)

```csharp
var activeSections = context.Sections
    .AsEnumerable()  // forces all rows to load into memory
    .Where(s => s.Schedule.Contains(today))
    .ToList();
```

```sql
-- No WHERE clause — loads everything
SELECT [s].[Id], [s].[CourseId], [s].[InstructorId], [s].[SectionName],
       [s].[Schedule_EndDate], [s].[Schedule_StartDate]
FROM [Sections] AS [s]
```

`AsEnumerable()` materializes all rows, then C# filters in memory. The `Contains()` method works because it's now running as regular C#, not being translated to SQL.

**Warning:** This loads the entire table. With 11 rows it's fine; with millions, it's a performance disaster.

---

## Comparison Table

| Approach | Server-Side Query | Performance | Complexity | Limitations |
|---|:---:|:---:|:---:|---|
| **Owned Entity (OwnsOne)** | Yes | Best | Low | Only for persisted properties |
| **Value Converter (HasConversion)** | Equality only | Good | Low | Can't access inner members in LINQ |
| **Rewrite with persisted props** | Yes | Best | Medium | Must manually decompose the logic |
| **HasComputedColumnSql** | Yes | Good | Low | Same-row only; formula in two places |
| **Client-side (AsEnumerable)** | No | Worst | Low | Loads all rows into memory |

## Rules of Thumb

1. **If the value object maps to columns** (`OwnsOne`) — query its persisted properties freely. They translate to SQL.
2. **If the value object maps to a single column** (`HasConversion`) — use equality (`==`) comparisons. Don't try to access inner properties in LINQ.
3. **If you need to query a computed value** — rewrite the LINQ using the underlying persisted properties. This is almost always the right answer.
4. **`HasComputedColumnSql`** is useful when the computation is simple and you want to query it naturally, but accept the maintenance cost of keeping C# and SQL in sync.
5. **Client-side evaluation** is a last resort — only use it for small datasets or when no server-side alternative exists.

## File Structure

```
10-Code-first/
  Entites/
    ValueObjects/
      Address.cs           # Owned entity (OwnsOne) — on Instructor
      CourseTag.cs          # Value converter (HasConversion) — on Course
      DateRange.cs          # Owned entity with computed properties — on Section
    Course.cs              # Has required CourseTag Tag
    Instructor.cs          # Has Address? Address
    Section.cs             # Has DateRange Schedule
  Data/Config/
    InstructorConfiguration.cs  # OwnsOne + seed data
    CourseConfiguration.cs      # HasConversion + seed data
    SectionConfiguration.cs     # OwnsOne + Ignore() + seed data
  Examples/
    ValueObjectExamples.cs      # Examples 1-4 (owned entities, value converters)
    ComputedPropertyExamples.cs # Examples 5-8 (computed property problem + alternatives)
  Migrations/
    ...AddInstructorAddress.cs
    ...AddCourseTag.cs
    ...AddSectionSchedule.cs
```

## Migration Commands

Using Package Manager Console (default project: `10-Code-first`):

```
Add-Migration AddInstructorAddress
Update-Database

Add-Migration AddCourseTag
Update-Database

Add-Migration AddSectionSchedule
Update-Database
```

Using dotnet CLI:

```bash
dotnet ef migrations add AddInstructorAddress --project 10-Code-first
dotnet ef database update --project 10-Code-first

dotnet ef migrations add AddCourseTag --project 10-Code-first
dotnet ef database update --project 10-Code-first

dotnet ef migrations add AddSectionSchedule --project 10-Code-first
dotnet ef database update --project 10-Code-first
```

## Forward Note: EF Core 8+ ComplexProperty

This project uses EF Core 7.0.5. Starting with **EF Core 8**, `ComplexProperty` was introduced as an alternative to `OwnsOne` for value objects. The key difference: `ComplexProperty` does not allow the value to be null — it's always present on the entity. If your value object is truly required (not optional), `ComplexProperty` may be a better semantic fit. The queryability rules are the same.
