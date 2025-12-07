

using EF09.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EF09.Data;

internal class AppDbContext : DbContext
{
   
    
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .ToTable("Products", schema: "Inventory")
                .HasKey(x => x.Id);

            modelBuilder.Entity<Order>()
           .ToTable("Orders", schema: "Sales")
           .HasKey(x => x.Id);

            modelBuilder.Entity<OrderDetail>()
              .ToTable("OrderDetails", schema: "Sales")
              .HasKey(x => x.Id);

            // modelBuilder.HasDefaultSchema("Sales");

            base.OnModelCreating(modelBuilder);


        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .Build();

        var constr = configuration.GetConnectionString("Resto");

            optionsBuilder.UseSqlServer(constr);
        }
    
}
