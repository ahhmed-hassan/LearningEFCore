using EFTest.Data.BlogDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EFTest
{
    internal class Program
    {
        internal static IConfigurationRoot configuration => new ConfigurationBuilder().
                AddJsonFile("appsettings.json").
                Build();
       
        static async Task SeedData()
        {
            using (var context = new BlogDbContext())
            {
                await DataSeeder.SeedAsync(context);
            }
        }

        static async Task Main(string[] args)
        {

            bool seed = configuration.GetValue<bool>("SeedDatabase");
            if (seed)
                await SeedData(); 
            
            using ( var context = new BlogDbContext())
            {
                var fristcomment = await context.Comments.FindAsync(1);
            }

            using (var context = new Data.FakeTwitterV2.FakeTwitterV2DBContext())
            {
                var firstuser = await context.users.FirstAsync();
                Console.WriteLine( firstuser.Username);
            }

        }
    }
}
