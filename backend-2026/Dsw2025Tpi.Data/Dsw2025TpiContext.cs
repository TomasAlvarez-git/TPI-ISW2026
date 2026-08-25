using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext: DbContext
{
    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) 
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.Property(e => e.Id).HasMaxLength(36).IsRequired();
            e.Property(e => e.Sku).HasMaxLength(8).IsRequired();
            e.Property(e => e.InternalCode).HasMaxLength(8).IsRequired();
            e.Property(e => e.Name).HasMaxLength(50).IsRequired();
            e.Property(e => e.Description).HasMaxLength(80);
            e.Property(e => e.CurrentUnitPrice).HasPrecision(15, 2).IsRequired();
            e.Property(e => e.StockQuantity).HasMaxLength(3).IsRequired();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.Property(e => e.Id).HasMaxLength(36).IsRequired();
            e.Property(e => e.Date).IsRequired();
            e.Property(e => e.ShippingAddress).HasMaxLength(50).IsRequired();
            e.Property(e => e.BillingAddress).HasMaxLength(50).IsRequired();
            e.Property(e => e.Notes).HasMaxLength(100);
            e.Property(e => e.Status).IsRequired();
            e.Property(e => e.CustomerId).IsRequired();
            e.HasMany(e => e.OrderItems)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.Property(e => e.Id).HasMaxLength(36).IsRequired();
            e.Property(e => e.ProductId).HasMaxLength(36).IsRequired();
            e.Property(e => e.Description).HasMaxLength(50);
            e.Property(e => e.Quantity).IsRequired();
            e.Property(e => e.UnitPrice).HasPrecision(15, 2).IsRequired();
        });
 
    modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("Customers");
            e.Property(e => e.Id).HasMaxLength(36).IsRequired();
            e.Property(e => e.Email).HasMaxLength(50).IsRequired();
            e.Property(e => e.Name).HasMaxLength(50).IsRequired();
            e.Property(e => e.PhoneNumber).HasMaxLength(15);
            e.HasMany(e => e.Orders)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
