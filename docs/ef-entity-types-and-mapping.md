# Entity Types and Mapping Study Guide

**Branch:** `ef/entity-types-and-mapping`

## Overview
This branch demonstrates Entity Framework Core fundamentals focusing on:
- Defining entity types
- Configuring entities using `IEntityTypeConfiguration`
- Setting up relationships between entities
- Using Fluent API for entity configuration
- Working with navigation properties

## Project Structure: EFTest

### Domain Models

#### 1. Blog Domain (`EFTest/Domain/Blog`)

Three main entities demonstrating a typical blogging system:

**User Entity** (`User.cs`)
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public List<Post> Posts { get; set; } = new();
}
```

**Post Entity** (`Post.cs`)
```csharp
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public User Author { get; set; }
    public List<Comment> Comments { get; set; } = new();
}
```

**Comment Entity** (`Comment.cs`)
```csharp
public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public Post Post { get; set; }
    public User Author { get; set; }
}
```

### Entity Configurations

#### UserConfiguration (`UserConfigration.cs`)

Key concepts demonstrated:
```csharp
public void Configure(EntityTypeBuilder<User> builder)
{
    // Primary key configuration
    builder.HasKey(u => u.Id);

    // Property constraints
    builder.Property(u => u.Username)
        .IsRequired()
        .HasMaxLength(50);

    builder.Property(u => u.Email)
        .IsRequired()
        .HasMaxLength(100);

    // Unique index
    builder.HasIndex(u => u.Email)
        .IsUnique();

    // One-to-Many relationship: User has many Posts
    builder.HasMany(u => u.Posts)
        .WithOne(p => p.Author)
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

**Key Learnings:**
- `HasKey()`: Defines primary key
- `IsRequired()`: Makes property non-nullable in database
- `HasMaxLength()`: Sets maximum string length
- `HasIndex().IsUnique()`: Creates unique constraint
- `HasMany().WithOne()`: Configures one-to-many relationship
- `OnDelete(DeleteBehavior.Cascade)`: Deletes all posts when user is deleted

#### PostConfiguration (`PostConfigration.cs`)

```csharp
public void Configure(EntityTypeBuilder<Post> builder)
{
    builder.HasKey(p => p.Id);

    builder.Property(p => p.Title)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(p => p.Content)
        .IsRequired();

    // Post has many Comments
    builder.HasMany(p => p.Comments)
        .WithOne(c => c.Post)
        .HasForeignKey(c => c.PostId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

**Key Learnings:**
- Cascade delete propagates to comments when post is deleted

#### CommentConfiguration (`CommentConfigration.cs`)

```csharp
public void Configure(EntityTypeBuilder<Comment> builder)
{
    builder.HasKey(c => c.Id);

    builder.Property(c => c.Text)
        .IsRequired()
        .HasMaxLength(500);

    // Relationship to User (Author)
    builder.HasOne(c => c.Author)
        .WithMany() // User doesn't have a Comments navigation property
        .HasForeignKey(c => c.UserId)
        .OnDelete(DeleteBehavior.Restrict);
}
```

**Key Learnings:**
- `WithMany()` without argument: The parent entity doesn't need a collection navigation property
- `OnDelete(DeleteBehavior.Restrict)`: Prevents deleting a user if they have comments (preserves comment history)

### DbContext Configuration

**BlogDbContext** (`BlogDbContext.cs`)
```csharp
public class BlogDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);
    }
}
```

**Key Learnings:**
- `ApplyConfigurationsFromAssembly()`: Automatically finds and applies all `IEntityTypeConfiguration` implementations
- `LogTo()`: Logs SQL queries to console for debugging

#### 2. Twitter Domain (`EFTest/Domain/TwitterV2`)

Simpler entities demonstrating primary key conventions:

**User Entity**
```csharp
public class User
{
    // Primary key convention: {Class}Id
    public int UserId { get; set; }
    public required string Username { get; set; }
}
```

**Tweet Entity**
```csharp
public class Tweet
{
    public int TweetId { get; set; }
    public int UserId { get; set; }
    public required string TweetText { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Key Learnings:**
- EF Core recognizes `{EntityName}Id` as primary key by convention (not just `Id`)

### Database Contexts

Three separate contexts to demonstrate different database configurations:

1. **BlogDbContext**: Fully configured with relationships
2. **FakeTwitterV1DBContext**: Basic setup with conventions
3. **FakeTwitterV2DBContext**: Alternative Twitter implementation

## Key Concepts Covered

### 1. Entity Type Configuration
- Using `IEntityTypeConfiguration<T>` interface
- Separating configuration from entity classes
- Fluent API methods

### 2. Relationships
- **One-to-Many**: User → Posts, Post → Comments
- **Many-to-One**: Post → User (Author), Comment → User (Author)
- Navigation properties (reference and collection)
- Foreign key configuration

### 3. Property Constraints
- Required fields (`IsRequired()`)
- String length limits (`HasMaxLength()`)
- Unique constraints (`HasIndex().IsUnique()`)

### 4. Delete Behaviors
- `Cascade`: Delete children when parent is deleted
- `Restrict`: Prevent parent deletion if children exist
- `SetNull`: Set foreign key to null when parent is deleted

### 5. Primary Key Conventions
- `Id` property
- `{ClassName}Id` property
- Explicit configuration with `HasKey()`

## Practical Exercises

1. Add a `Like` entity that relates to both Post and User
2. Implement a `Tag` system with many-to-many relationship to Posts
3. Add validation rules (email format, username uniqueness)
4. Implement soft delete for Users and Posts

## Common Patterns

- Separate domain models from configuration
- Use `ApplyConfigurationsFromAssembly()` for clean DbContext
- Configure bidirectional relationships from one side only
- Use appropriate delete behaviors based on business rules
