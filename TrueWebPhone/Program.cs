using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrueWebPhone.Models;

namespace TrueWebPhone
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql("Server=localhost;Database=truewebphone;User ID=root;Password=;",
                     new MariaDbServerVersion(new Version(10, 4, 28)))
           .EnableSensitiveDataLogging()  // Add this line for detailed query logging
           .LogTo(Console.WriteLine, LogLevel.Information));  // Log to console
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Forbidden";
                });

            var app = builder.Build();

            app.UseStatusCodePagesWithReExecute("/Error/{0}");


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            // Get the service provider from the built application
            using (var serviceScope = app.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;

                // Get the DbContext from the service provider
                var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

                // Apply database migration and update during application startup
                dbContext.Database.Migrate();
                CreateNewUserAccount(dbContext, "admin@gmail.com", "Admin");
            }

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
        private static void CreateNewUserAccount(ApplicationDbContext dbContext, string email, string name)
        {
            if (!dbContext.Accounts.Any(u => u.Username == "admin"))
            {
                string username = email.Substring(0, email.IndexOf('@'));
                string passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

                var newAccount1 = new Account
                {
                    Email = email,
                    Username = username,
                    Password = passwordHash,
                    Image = "uploads/account/default.png",
                    Role = "Admin",
                    isChangePass = true,
                    Status = "Active",
                    Name = name,
                };


                dbContext.Accounts.Add(newAccount1);
                dbContext.SaveChanges();
            }
        }

    }

}

