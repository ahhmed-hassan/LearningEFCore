# TODO List

## Database Renaming (Pending)

**Problem:** The database names don't match their purposes after reorganization.

**Current state:**
- `MetigatorCFM` — contains all migrations including value-objects (used by `ef/value-objects` branch)
- `MetigatorOneToMany` — fresh database with only one-to-many migrations (used by `ef/one-to-many` branch)

**Desired state:**
- `MetigatorValueObjects` — for `ef/value-objects` branch (currently named `MetigatorCFM`)
- `MetigatorCFM` — shared by `ef/code-first`, `ef/entity-types-and-mapping`, `ef/one-to-many` (currently named `MetigatorOneToMany`)

**Steps to complete:**

1. **Rename databases in SQL Server** (run in DBeaver or any SQL client):
   ```sql
   -- Step 1: Rename MetigatorCFM to MetigatorValueObjects
   ALTER DATABASE MetigatorCFM SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
   ALTER DATABASE MetigatorCFM MODIFY NAME = MetigatorValueObjects;
   ALTER DATABASE MetigatorValueObjects SET MULTI_USER;

   -- Step 2: Rename MetigatorOneToMany to MetigatorCFM
   ALTER DATABASE MetigatorOneToMany SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
   ALTER DATABASE MetigatorOneToMany MODIFY NAME = MetigatorCFM;
   ALTER DATABASE MetigatorCFM SET MULTI_USER;
   ```

2. **Update connection strings in `EF09/appsettings.json`:**
   ```json
   "MetigatorCFM": "Server=localhost,1432;Database=MetigatorCFM;...",
   "MetigatorValueObjects": "Server=localhost,1432;Database=MetigatorValueObjects;..."
   ```
   Remove `MetigatorOneToMany` entry.

3. **Update `AppDbContext.cs` on each branch:**
   - `ef/code-first` → change to use `MetigatorCFM`
   - `ef/entity-types-and-mapping` → change to use `MetigatorCFM`
   - `ef/one-to-many` → already uses `MetigatorOneToMany`, change to `MetigatorCFM`
   - `ef/value-objects` → change to use `MetigatorValueObjects`

4. **Run `Update-Database` on each branch** to ensure migrations are in sync with the renamed databases.

5. **Commit and push** all branches after verification.

---

## Other Items

_(Add future TODOs here)_
