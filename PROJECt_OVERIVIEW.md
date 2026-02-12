# EF Core Learning Project Overview

## Infrastructure

Docker Compose runs **SQL Server 2022** on port `1432` with a persistent volume. All three EF Core projects connect to it.

```bash
# Spin up SQL Server
docker-compose up -d
```

All connection strings use `sa` / `YourStrong!Pass123` on `localhost,1432`.

---

## Modules

### 1. EF09 — Schema Routing & Data Annotations

The simplest module. Three entities: **Product**, **Order**, **OrderDetail** forming a classic order system.

- **Mix of approaches**: `[Table]` and `[Key]` data annotations on `Order`, plus Fluent API in `OnModelCreating` for schema routing
- **Schema separation**: Products go to `"Inventory"` schema, Orders/OrderDetails go to `"Sales"` schema
- **Relationships**: Order -> OrderDetail (one-to-many), OrderDetail -> Product (many-to-one)
- EF Core **7.0.3**, database: `Resto`
- **No migrations and no `EnsureCreated`** — the `Resto` database must be created manually or by adding migrations

### 2. EFTest — Multiple DbContexts & Advanced Relationships

The most feature-rich module. Contains **3 separate domains** each with their own DbContext.

#### Blog Domain (main)

- **User -> Post -> Comment** hierarchy with full Fluent API via `IEntityTypeConfiguration<T>` classes
- Configs loaded automatically with `ApplyConfigurationsFromAssembly()`
- Cascade delete on User->Posts and Post->Comments, but **Restrict** on Comment->Author (avoids multiple cascade paths)
- **DataSeeder** class that calls `EnsureDeletedAsync()` + `EnsureCreatedAsync()` then seeds Users/Posts/Comments
- Toggled by `"SeedDatabase"` flag in appsettings.json
- **Warning**: Seeding is destructive — it drops and recreates the entire `BlogDb` database. Set back to `false` after seeding.

#### FakeTwitterV1 & V2

- Convention-only contexts (no `OnModelCreating`) — demonstrates how EF Core infers keys from `{ClassName}Id` naming
- **No `EnsureCreated` call** — `FakeTwitterV2` database won't exist until you add one. Running Program.cs will crash on the FakeTwitterV2 query.
- V1 has a connection string typo pointing to V2's database
- EF Core **9.0.11**, multiple databases: `BlogDb`, `FakeTwitterV2`, etc.

### 3. 10-Code-first — Migrations & Seeding in Configs

Demonstrates the **full migration workflow**: **Course**, **Instructor**, **Office** entities.

- All configs use `IEntityTypeConfiguration<T>` with `ValueGeneratedNever()` (manual IDs)
- **One-to-one**: Office <-> Instructor (FK on `Instructor.OfficeId`, optional)
- **3 migrations** showing evolution:
  1. `Initial` — creates Courses + Instructors, seeds data
  2. `Separate Name` — splits `Name` column into `FirstName`/`LastName` with data migration
  3. `add-office-entity` — adds Office entity, links existing instructors
- Seeds data directly via `HasData()` inside configuration classes
- EF Core **7.0.5**, database: `MetigatorCFM`

---

## Patterns Comparison

| Pattern | EF09 | EFTest (Blog) | 10-Code-first |
|---|---|---|---|
| Config style | Mixed annotations + Fluent | Pure Fluent (`IEntityTypeConfiguration`) | Pure Fluent (`IEntityTypeConfiguration`) |
| Seeding | None | External `DataSeeder` class | `HasData()` in config classes |
| DB creation | Manual (no migrations) | `EnsureCreated` (no migrations) | Full migration chain |
| Key generation | Auto-increment | Auto-increment | `ValueGeneratedNever()` |
| String types | Default nvarchar | VARCHAR with max lengths | VARCHAR with max lengths |

---

## Quick Start

```bash
# 1. Start SQL Server
docker-compose up -d

# 2. Apply migrations for 10-Code-first (run from inside the project folder,
#    running from solution root fails due to docker-compose.dcproj interference)
cd 10-Code-first
dotnet ef database update
cd ..

# 3. For EFTest Blog, set "SeedDatabase": true in appsettings.json then run
dotnet run --project EFTest
# Then set "SeedDatabase" back to false (seeding is destructive — drops + recreates DB)
```

---

## Known Quirks

- **EF09**: No way to create `Resto` DB automatically — needs migrations added or manual creation
- **EFTest FakeTwitterV2**: Program.cs queries it but no `EnsureCreated` is called, so it crashes if that DB doesn't exist
- **FakeTwitterV1 connection string**: Points to `FakeTwitterV2` database (copy-paste typo in appsettings)
- **`dotnet ef` from solution root**: Fails because `docker-compose.dcproj` confuses the EF tooling. Run from inside the specific project folder instead.
