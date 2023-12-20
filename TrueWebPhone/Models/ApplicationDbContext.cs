using Microsoft.EntityFrameworkCore;
using TrueWebPhone.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) {
        
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOne(p => p.Customer)
            .WithMany(a => a.Orders)
            .HasForeignKey(p => p.CustomerId);

        modelBuilder.Entity<ProductOrder>()
        .HasKey(po => new { po.OrderId, po.ProductId });

        modelBuilder.Entity<ProductOrder>()
            .HasOne(po => po.Order)
            .WithMany(o => o.ProductOrders)
            .HasForeignKey(po => po.OrderId);

        modelBuilder.Entity<ProductOrder>()
            .HasOne(po => po.Product)
            .WithMany(p => p.ProductOrders)
            .HasForeignKey(po => po.ProductId);
    }
}
