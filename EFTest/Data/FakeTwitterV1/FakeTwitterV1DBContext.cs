using EFTest.Domain.TwitterV2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EFTest.Data.FakeTwitterV1;

internal class FakeTwitterV1DBContext : DbContext
{
    private static IConfigurationRoot configuration => new ConfigurationBuilder().
                AddJsonFile("appsettings.json").
                Build();

    public DbSet<User> users { get; set; }
    public DbSet<Tweet> tweets { get; set; }
    public DbSet<Comment> comments { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("FakeTwitterV1")).
            LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            ;
    }
}
