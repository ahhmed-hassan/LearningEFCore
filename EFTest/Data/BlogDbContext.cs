using EFTest.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTest.Data;
internal class BlogDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.UseSqlite("Data Source=blog.db");
        optionsBuilder.UseSqlServer("Server=localhost,1432;Database=BlogDb;User Id=sa;Password=YourStrong!Pass123;TrustServerCertificate=True");
           
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //// Configure relationships
        //modelBuilder.Entity<Post>()
        //    .HasOne(p => p.Author)
        //    .WithMany(u => u.Posts)
        //    .HasForeignKey(p => p.UserId);

        //modelBuilder.Entity<Comment>()
        //    .HasOne(c => c.Post)
        //    .WithMany(p => p.Comments)
        //    .HasForeignKey(c => c.PostId);

        //modelBuilder.Entity<Comment>()
        //    .HasOne(c => c.Author)
        //    .WithMany()
        //    .HasForeignKey(c => c.UserId);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);
    }
}

