using EFTest.Data;
using EFTest.Domain;
using Microsoft.IdentityModel.Tokens;

namespace EFTest
{
    internal class Program
    {
        static async Task SeedData()
        {
            using (var context = new BlogDbContext())
            {
                // Recreate database fresh each time (for experimentation)
                // await context.Database.EnsureDeletedAsync();
                //await context.Database.EnsureCreatedAsync();
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                // Seed users
                var users = new[]
                {
                    new User { Username = "alice", Email = "alice@example.com", CreatedAt = DateTime.UtcNow.AddDays(-30) },
                    new User { Username = "bob", Email = "bob@example.com", CreatedAt = DateTime.UtcNow.AddDays(-25) },
                    new User { Username = "charlie", Email = "charlie@example.com", CreatedAt = DateTime.UtcNow.AddDays(-20) }
                };
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Seed posts
                var posts = new[]
                {
                    new Post { Title = "Getting Started with EF Core", Content = "EF Core is amazing...", UserId = users[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-15) },
                    new Post { Title = "SQLite for Development", Content = "SQLite is perfect for local dev...", UserId = users[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                    new Post { Title = "C# Tips and Tricks", Content = "Here are some useful patterns...", UserId = users[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                    new Post { Title = "Database Design 101", Content = "Normalization is key...", UserId = users[2].Id, CreatedAt = DateTime.UtcNow.AddDays(-3) }
                };
                context.Posts.AddRange(posts);
                await context.SaveChangesAsync();

                // Seed comments
                var comments = new[]
                {
                    new Comment { Text = "Great article!", PostId = posts[0].Id, UserId = users[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-14) },
                    new Comment { Text = "Thanks for sharing", PostId = posts[0].Id, UserId = users[2].Id, CreatedAt = DateTime.UtcNow.AddDays(-13) },
                    new Comment { Text = "Very helpful", PostId = posts[1].Id, UserId = users[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-9) },
                    new Comment { Text = "I disagree with point 3", PostId = posts[2].Id, UserId = users[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                    new Comment { Text = "Could you elaborate?", PostId = posts[3].Id, UserId = users[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                    new Comment { Text = "Do you think I care?", PostId = posts[2].Id, UserId = users[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-2) }

                };
                context.Comments.AddRange(comments);
                await context.SaveChangesAsync();

                Console.WriteLine("Database seeded successfully!");
            }
        }

        static async Task Main(string[] args)
        {
            const bool seed = true;
            if (seed)
                await SeedData(); 
            
            using ( var context = new BlogDbContext())
            {
                
            }

        }
    }
}
