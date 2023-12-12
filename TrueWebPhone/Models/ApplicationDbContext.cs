using Microsoft.EntityFrameworkCore;
using TrueWebPhone.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) {
        
    }

    public DbSet<Account> Accounts { get; set; }

    

}
