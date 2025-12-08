using _10_Code_first.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace _10_Code_first.Data;

public class AppDbContext : DbContext
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Office> Offices { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        var configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();
        var constr = configuration.GetConnectionString("MetigatorCFM");
        optionsBuilder.UseSqlServer(constr)
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            
            ;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
