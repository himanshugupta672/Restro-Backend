using Microsoft.EntityFrameworkCore;
using Restaurant.Domain.Entities;

namespace Restaurant.Infrastructure.Data;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }

    public DbSet<MenuItem> MenuItems { get; set; }

    public DbSet<Table> Tables { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<Chef> Chefs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

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

        modelBuilder.Entity<RefreshToken>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(x => x.TokenHash)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(x => x.Email)
            .HasMaxLength(256);

        modelBuilder.Entity<User>()
            .Property(x => x.PhoneNumber)
            .HasMaxLength(32);

        modelBuilder.Entity<Table>()
            .Property(x => x.Token)
            .HasMaxLength(450);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => x.PhoneNumber)
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL");

        modelBuilder.Entity<Table>()
            .HasIndex(x => x.TableNumber)
            .IsUnique();

        modelBuilder.Entity<Table>()
            .HasIndex(x => x.Token)
            .IsUnique();

        modelBuilder.Entity<MenuItem>()
            .HasIndex(x => x.CategoryId);

        modelBuilder.Entity<Order>()
            .HasIndex(x => x.TableId);

        modelBuilder.Entity<Order>()
            .HasIndex(x => x.ChefId);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Chef>()
            .Property(x => x.Name)
            .HasMaxLength(150);

        modelBuilder.Entity<Chef>()
            .Property(x => x.PhoneNumber)
            .HasMaxLength(32);

        modelBuilder.Entity<Chef>()
            .Property(x => x.Email)
            .HasMaxLength(256);

        modelBuilder.Entity<Chef>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Chef>()
            .HasIndex(x => x.PhoneNumber)
            .IsUnique();
    }
}
