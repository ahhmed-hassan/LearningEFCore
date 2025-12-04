using EFTest.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTest.Data;
internal class BlogDbContext : DbContext
{
    private static IConfigurationRoot configuration => new ConfigurationBuilder().
                AddJsonFile("appsettings.json").
                Build();
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);
           
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

