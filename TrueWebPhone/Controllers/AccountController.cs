using System.Net.Mail;
using System.Net;
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

    //vclcvlclvclvlcvc
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var currentUsername = User.Identity.Name;

        var accounts = await ct.Accounts
            .Where(a => a.Username != currentUsername && a.Role != "Admin")
            .ToListAsync();

        return View(accounts);

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

        if (account.Username == "admin" && account.Password == "123456")
        {
            if (ct.Accounts.Any(a => a.Username == "admin"))
            {
                await SaveLogin(account.Username, account.Password);
            }
            else
            {
                await RegisterAdmin("admin@gmail.com", "admin");
                await SaveLogin(account.Username, account.Password);
            }
        }
        else
        {
            await validateLogin(account.Username, account.Password);
        }

        return View();
    }

    private async Task validateLogin(string username, string password)
    {
        var account = await ct.Accounts.SingleOrDefaultAsync(a => a.Username == username);

        if (account != null && BCrypt.Net.BCrypt.Verify(password, account.Password))
        {
            await SaveLogin(username, password);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password");
            View("Login");
        }
    }

    public async Task RegisterAdmin(string email, string name)
    {
        if (string.IsNullOrEmpty(email) || email.IndexOf('@') == -1)
        {
            ModelState.AddModelError("Email", "Invalid email address");
        }

        string username = email.Substring(0, email.IndexOf('@'));
        string passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");


        var newAccount = new Account
        {
            Email = email,
            Username = username,
            Password = passwordHash,
            Image = "default.jpg",
            Role = "Admin",
            Status = "Active",
            Name = name,
        };

        ct.Accounts.Add(newAccount);
        await ct.SaveChangesAsync();
    }

    private async Task SaveLogin(String username, String password)
    {
        String role = "User";
        if (username.Contains("admin")) role = "Admin";
        else role = "Seller";


        var claims = new List<Claim>() {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),

        };

        var indentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(indentity);

        await HttpContext.SignInAsync(principal);

        string returnUrl = "/";  

        Response.Redirect(returnUrl);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var account = await ct.Accounts.FindAsync(id);

        if (account == null)
        {
            return NotFound();
        }

        return View(account);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Username,Password,Image,Role,Status,Name")] Account updatedAccount)
    {
        if (id != updatedAccount.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingAccount = await ct.Accounts.FindAsync(id);

                if (existingAccount == null)
                {
                    return NotFound();
                }

                existingAccount.Email = updatedAccount.Email;
                existingAccount.Username = updatedAccount.Username;
                existingAccount.Password = updatedAccount.Password;
                existingAccount.Image = updatedAccount.Image;
                existingAccount.Role = updatedAccount.Role;
                existingAccount.Status = updatedAccount.Status;
                existingAccount.Name = updatedAccount.Name;

                await ct.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        return View(updatedAccount);
    }

    // Helper method to check if an account exists
    private bool AccountExists(int id)
    {
        return ct.Accounts.Any(a => a.Id == id);
    }


    public IActionResult Logout()
    {
        HttpContext.SignOutAsync();
        string returnUrl = "/";

        return Redirect("/");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Register()
    {
       
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(string email, string name)
    {
        if (string.IsNullOrEmpty(email) || email.IndexOf('@') == -1)
        {
            // Handle invalid email address
            ModelState.AddModelError("Email", "Invalid email address");
            return View();
        }
        if (ct.Accounts.Any(a => a.Email == email))
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View();
        }

        string username = email.Substring(0, email.IndexOf('@'));
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(username);

        var newAccount = new Account
        {
            Email = email,
            Username = username,
            Password = passwordHash,
            Image = "default.jpg",
            Role = "Seller",
            Status = "InActive",
            Name = name,
        };

        ct.Accounts.Add(newAccount);
        await ct.SaveChangesAsync();

        var activationLink = Url.Action("ActivateAccount", "Account", new { id = newAccount.Id}, Request.Scheme);

        await SendEmailAsync(email, "Confirm Account", activationLink);


        return RedirectToAction("Index");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ActivateAccount(int id)
    {
        var account = await ct.Accounts.FindAsync(id);

        if (account == null)
        {
            return View("InvalidActivationLink");
        }

        // Activate the account
        account.Status = "Active";
        await ct.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    private async Task<bool> SendEmailAsync(string email, string subject, string confirmlink)
    {
        try
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("vate202@gmail.com");
            message.To.Add(email);
            message.Subject = subject;
            message.Body = $"Click the following link to activate your account: {confirmlink}";
            message.IsBodyHtml = true;

            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;

            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("vate202@gmail.com", "lktyqjjjbiyefldc");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

            smtp.Send(message);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public IActionResult Forbidden()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Search(string searchString)
    {
        var currentUsername = User.Identity.Name;

        var accounts = ct.Accounts
            .Where(a => a.Username != currentUsername && a.Role != "Admin")
            .ToList();

        if (!string.IsNullOrEmpty(searchString))
        {
            accounts = accounts.Where(a =>
                a.Email.Contains(searchString) ||
                a.Username.ToLower().Contains(searchString) ||
                a.Name.ToLower().Contains(searchString) ||
                a.Role.ToLower().Contains(searchString) ||
                a.Status.ToLower().Contains(searchString)
            // Add additional fields you want to search here
            ).ToList();
        }

        return View("Index", accounts);
    }
}