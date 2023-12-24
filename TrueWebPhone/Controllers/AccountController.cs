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
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Hosting;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TrueWebPhone.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext ct;
    private readonly IWebHostEnvironment _webHostEnvironment;


    public AccountController(ApplicationDbContext ct, IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;

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

    [HttpGet]

    [AllowAnonymous]
    public async Task<IActionResult> Profile()
    {
        var currentUsername = User.Identity.Name;

        var account = await ct.Accounts
            .FirstOrDefaultAsync(a => a.Username == currentUsername);

        if (account == null)
        {
            return NotFound();
        }

        return View(account);
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
        account.AccountImage = null;
        if (!ModelState.IsValid)
        {
            return View();
        }

        var result = await validateLogin(account.Username, account.Password);

        if (result == LoginResult.Success)
        {
            // Check if the user needs to change their password
            var user = await ct.Accounts.SingleOrDefaultAsync(a => a.Username == account.Username);
            if (user != null && !user.isChangePass)
            {
                return RedirectToAction("ChangePassword", new { id = user.Id });
            }

            return Redirect("/");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password");
            return View("Login");
        }
    }

    private async Task<LoginResult> validateLogin(string username, string password)
    {
        var account = await ct.Accounts.SingleOrDefaultAsync(a => a.Username == username);

        if (account != null && BCrypt.Net.BCrypt.Verify(password, account.Password))
        {
            await SaveLogin(username, password);
            return LoginResult.Success;
        }
        else
        {
            return LoginResult.Failure;
        }
    }

    public enum LoginResult
    {
        Success,
        Failure
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
            Image = "uploads/account/default.png",
            Role = "Admin",
            isChangePass = true,
            Status = "Active",
            Name = name,
        };

        ct.Accounts.Add(newAccount);
        await ct.SaveChangesAsync();
    }

   private async Task SaveLogin(string username, string password)
{
    var account = await ct.Accounts.SingleOrDefaultAsync(a => a.Username == username);

    if (account != null && BCrypt.Net.BCrypt.Verify(password, account.Password))
    {
        if (!account.isChangePass)
        {
                RedirectToAction("ChangePassword", new { id = account.Id });
                // Instead of returning, you can perform the redirect here
                var redirectResult = RedirectToAction("ChangePassword", new { id = account.Id });
                await redirectResult.ExecuteResultAsync(ControllerContext);
                return;
            }

            string role = "User";
        if (username.Contains("admin")) role = "Admin";
        else role = "Seller";

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(principal);

        string returnUrl = "/";  

        Response.Redirect(returnUrl);
    }
    else
    {
        ModelState.AddModelError(string.Empty, "Invalid username or password");
        View("Login");
    }
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
    public IActionResult ChangePassword(int id)
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(int id, string oldPass, string newPass, string confPass)
    {
        var account = await ct.Accounts.FindAsync(id);

        if (account == null)
        {
            return NotFound();
        }

        if (!BCrypt.Net.BCrypt.Verify(oldPass, account.Password))
        {
            ModelState.AddModelError("OldPassword", "Incorrect old password");
            return View(); 
        }

        if (newPass != confPass)
        {
            ModelState.AddModelError("ConfirmPassword", "New password and confirm password do not match");
            return View(); 
        }

        account.Password = BCrypt.Net.BCrypt.HashPassword(newPass);
        account.isChangePass = true;
        await ct.SaveChangesAsync();

        TempData["SuccessMessage"] = "Password changed successfully";


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
            Image = "uploads/account/default.png",
            Role = "Seller",
            Status = "InActive",
            isChangePass = false,
            Name = name,
        };

        ct.Accounts.Add(newAccount);
        await ct.SaveChangesAsync();

        Account createAcc = ct.Accounts.FirstOrDefault(a => a.Username == newAccount.Username);

        var activationLink = Url.Action("ActivateAccount", "Account", new { id = newAccount.Id}, Request.Scheme);

        await SendEmailAsync(email, "Confirm Account",  createAcc.Id );


        return RedirectToAction("Index");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ActivateAccount(int id, long timestamp)
    {
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (currentTimestamp - timestamp > 60)
        {
            return View("InvalidActivationLink");
        }

        var account = await ct.Accounts.FindAsync(id);

        if (account == null)
        {
            return View("InvalidActivationLink");
        }

        account.Status = "Active";

        await ct.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult InvalidActivationLink()
    {
        return View();
    }



    private async Task<bool> SendEmailAsync(string email, string subject, int accountId)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var confirmlink = Url.Action("ActivateAccount", "Account", new { id = accountId, timestamp }, Request.Scheme);

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

    [HttpPost]
    public async Task<IActionResult> ChangePicture(int id, IFormFile newPicture)
    {
        try
        {
            var account = await ct.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }

            // Check if there is an existing image
            if (!string.IsNullOrEmpty(account.Image))
            {
                // Delete the old image file
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/account/", account.Image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            if (newPicture != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/account/");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(newPicture.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await newPicture.CopyToAsync(fileStream);
                }

                // Save the new picture to the specified path
                account.Image ="uploads/account/" + uniqueFileName;

                await ct.SaveChangesAsync();

                // Log success information

                return RedirectToAction("Profile");
            }
            else
            {
                // Log an error if the file is null or empty
                Console.WriteLine("Error: The provided file is null or empty.");
                return RedirectToAction("Profile");
            }
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the process
            Console.WriteLine($"Error: {ex.Message}");
            return RedirectToAction("Profile");
        }
    }




}