using Microsoft.EntityFrameworkCore;
using Restaurant.Domain.Entities;

namespace Restaurant.Infrastructure.Data;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }

    public DbSet<MenuItem> MenuItems { get; set; }

    public DbSet<Table> Tables { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<Chef> Chefs { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(o => o.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<MenuItem>()
    .Property(x => x.Price)
    .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Order>()
    .Property(o => o.Status)
    .HasConversion<string>();
    }
}