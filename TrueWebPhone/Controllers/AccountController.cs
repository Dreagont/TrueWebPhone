using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TrueWebPhone.Controllers;
using TrueWebPhone.Models;

namespace TrueWebPhone.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext ct;

    public AccountController(ApplicationDbContext ct)
    {
        this.ct = ct;
    }


    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View(await ct.Accounts.ToListAsync());
    }
    [OnlyUnauthenticated]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [OnlyUnauthenticated]
    public async Task<IActionResult> Login(Account account)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        // Check database for account login
        // Note: You should replace this with your actual authentication logic
        if (account.Username == "admin" && account.Password == "123456" ||
            account.Username == "manager" && account.Password == "123456" ||
            account.Username == "user" && account.Password == "123456")
        {
            await SaveLogin(account.Username, account.Password);
            return RedirectToAction("Index");
        }

        return View();
    }

    private async Task SaveLogin(String username, String password)
    {
        String role = "User";
        if (username.Contains("admin")) role = "Admin";
        else if (username.Contains("manager")) role = "Manager";


        var claims = new List<Claim>() {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),

        };

        var indentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(indentity);

        await HttpContext.SignInAsync(principal);
    }

    public IActionResult Logout()
    {
        HttpContext.SignOutAsync();
        return RedirectToAction("Index");
    }

    public IActionResult Register()
    {
       
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Register(string email, string name)
    {
        if (string.IsNullOrEmpty(email) || email.IndexOf('@') == -1)
        {
            // Handle invalid email address
            ModelState.AddModelError("Email", "Invalid email address");
            return View();
        }

        string username = email.Substring(0, email.IndexOf('@'));

        var newAccount = new Account
        {
            Email = email,
            Username = username,
            Password = username,
            Image = "default.jpg",
            Role = "Seller",
            Status = "InActive",
            Name = name,
        };

        ct.Accounts.Add(newAccount);
        await ct.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    public IActionResult Forbidden()
    {
        return View();
    }

}