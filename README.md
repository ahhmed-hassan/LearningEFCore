# Learning Entity Framework Core

A hands-on exploration of Entity Framework Core concepts through isolated examples and progressive learning branches.

## Repository Structure

This repository uses **branches as learning modules** - each branch demonstrates a specific EF Core concept with working code and documentation.

### Branch Overview

| Branch | Project | Focus | Documentation |
|--------|---------|-------|---------------|
| **ef/entity-types-and-mapping** | EFTest (SQLite) | Entity configurations, relationships, IEntityTypeConfiguration | [📖 Guide](docs/ef-entity-types-and-mapping.md) |
| **ef/code-first** | 10-Code-first | Migrations, schema evolution, data seeding | [📖 Guide](docs/ef-code-first.md) |
| **ef/one-to-many** | 10-Code-first | One-to-many relationships, required/optional FKs | [📖 Guide](docs/ef-one-to-many-relationships.md) |
| **master** | Multiple | Stable reference point | - |

## Quick Start

### Study a Specific Concept

```bash
# Example: Learn about entity configurations
git checkout ef/entity-types-and-mapping
cat docs/ef-entity-types-and-mapping.md  # Read the guide
cd EFTest
dotnet run

# Example: Learn about migrations
git checkout ef/code-first
cat docs/ef-code-first.md
cd 10-Code-first
dotnet run
```

### Navigate Between Topics

```bash
git branch -a                    # List all branches
git checkout <branch-name>       # Switch to a topic
```

## Branch Details

### 🔹 ef/entity-types-and-mapping

**Project:** `EFTest` (uses SQLite)

**What You'll Learn:**
- Defining entity types (User, Post, Comment)
- Implementing `IEntityTypeConfiguration<T>`
- Configuring properties (required, max length, unique indexes)
- Setting up one-to-many relationships
- Navigation properties
- Delete behaviors (Cascade, Restrict, SetNull)
- Primary key conventions

**Entities:** User, Post, Comment (Blog domain) + Tweet, User (Twitter domain)

**Database:** SQLite (portable, no SQL Server needed)

---

### 🔹 ef/code-first

**Project:** `10-Code-first` (Metigato Academy)

**What You'll Learn:**
- Code-first workflow
- Creating and applying migrations
- Migration commands (CLI and Package Manager Console)
- Schema evolution (3 migrations demonstrating progressive changes)
- Data seeding with `HasData()`
- Fluent API configuration patterns
- Rolling back migrations

**Entities:** Course, Instructor, Office

**Migrations:**
1. Initial (creates Course and Instructor tables)
2. Separate Name (splits Name into FirstName/LastName)
3. Add Office Entity (adds Office table and relationship)

**Database:** SQL Server

---

### 🔹 ef/one-to-many

**Project:** `10-Code-first` (extended)

**What You'll Learn:**
- One-to-many relationships (Course → Sections, Instructor → Sections)
- Required vs optional relationships
- Configuring relationships from parent entity
- Foreign key conventions
- Collection navigation properties
- Cascade delete vs SetNull
- Querying related data (Include, filtering)
- Shadow navigation properties

**Entities:** Course, Instructor, Office, **Section** (new)

**Key Relationships:**
- Course → Sections (required, cascade delete)
- Instructor → Sections (optional, set null on delete)

**Database:** SQL Server

---

## Documentation

Each branch contains detailed study guides in the `docs/` folder:

- **[Entity Types & Mapping](docs/ef-entity-types-and-mapping.md)** - Configurations and relationships
- **[Code-First Approach](docs/ef-code-first.md)** - Migrations and schema evolution
- **[One-to-Many Relationships](docs/ef-one-to-many-relationships.md)** - Relationship patterns

Documentation includes:
- ✅ Code examples with explanations
- ✅ Best practices and common pitfalls
- ✅ Practical exercises
- ✅ Command references (dotnet CLI + Package Manager Console)
- ✅ Real-world scenarios

## Prerequisites

- .NET 9.0 SDK
- SQL Server (for 10-Code-first project)
- SQLite (automatically included for EFTest project)

**Required Packages:**
```bash
# For migrations
dotnet tool install --global dotnet-ef

# In each project
dotnet restore
```

## Connection Strings

Update `appsettings.json` in each project with your database connection strings:

**EFTest:** Uses SQLite (file-based, no configuration needed)

**10-Code-first:** Requires SQL Server connection string

```json
{
  "ConnectionStrings": {
    "MetigatorCFM": "Server=.;Database=MetigatorAcademy;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Learning Path

**Recommended Order:**

1. **Start with ef/entity-types-and-mapping**
   - Simpler SQLite setup
   - Learn entity configuration fundamentals
   - Understand relationships basics

2. **Move to ef/code-first**
   - Learn migration workflow
   - Understand schema evolution
   - Practice with real migrations

3. **Progress to ef/one-to-many**
   - Build on code-first knowledge
   - Deep dive into relationships
   - See how migrations handle relationships

## Useful Commands

### Branch Navigation
```bash
git branch                       # List local branches
git checkout <branch-name>       # Switch branch
git log --oneline -10           # See recent commits
```

### Migrations (on ef/code-first or ef/one-to-many)
```bash
# .NET CLI
dotnet ef migrations list
dotnet ef database update

# Package Manager Console
Get-Migration
Update-Database
```

### Running Projects
```bash
cd EFTest          # or cd 10-Code-first
dotnet run
```

## Future Topics

Potential branches to add:
- Many-to-many relationships (Student ↔ Course enrollment)
- One-to-one relationships (Instructor ↔ Office)
- Value objects and owned entities
- Complex types
- Inheritance strategies (TPH, TPT, TPC)
- Query optimization and performance
- Change tracking
- Transactions and concurrency

## Organization Philosophy

**Why branches instead of folders?**
- ✅ Clean isolation - each concept has its own workspace
- ✅ Easy to experiment without breaking other examples
- ✅ Git history shows progression of learning
- ✅ Can easily reset/restart a topic
- ✅ Documentation stays in sync with code

**Why not merge everything?**
- Each branch serves as a "checkpoint" for a specific concept
- Easier to reference "the migrations example" vs "that part of the monolithic codebase"
- Can run each example independently
- Branches can be merged later if a unified codebase is needed

## Contributing to Your Learning

When adding a new concept:
1. Create a new branch: `git checkout -b ef/new-concept`
2. Implement the feature
3. Document it in `docs/ef-new-concept.md`
4. Commit documentation on the same branch
5. Update this README with the new branch

---

**Happy Learning! 🚀**

*Last Updated: 2026-02-12*
