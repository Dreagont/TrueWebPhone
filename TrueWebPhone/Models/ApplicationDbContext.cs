using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TrueWebPhone.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) {
        
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ProductOrder> ProductOrders { get; set; }
    public DbSet<Resend> Resends { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Phone)
                .IsUnique();


        modelBuilder.Entity<ProductOrder>().HasKey(po => po.ProductOrderId);

        base.OnModelCreating(modelBuilder);
    }
}

