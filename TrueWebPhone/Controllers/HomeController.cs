using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TrueWebPhone.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Lab7MVC.Controllers;

[Produces("application/json")]
public class HomeController : Controller
{

    private readonly ApplicationDbContext ct;

    public HomeController(ApplicationDbContext ct)
    {
        this.ct = ct;
    }

    
    
    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> CustomerList()
    {

        var accounts = await ct.Customers
            .ToListAsync();

        return View(accounts);

    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult CreateCustomer()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCustomer(string phone, string name, string address)
    {
        if (ModelState.IsValid)
        {
            Customer customer = new Customer(phone, name, address);
            ct.Customers.Add(customer);
            ct.SaveChanges();
            return Redirect("/");
        }

        foreach (var modelStateEntry in ModelState.Values)
        {
            foreach (var error in modelStateEntry.Errors)
            {
                Console.WriteLine($"Model error: {error.ErrorMessage}");
            }
        }

        return View();
    }






    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("/Error/{code:int}")] 
    public IActionResult HandleError(int code)
    {
        ViewData["ErrorMessage"] = $"Error occurred. The ErrorCode is: {code}";
        return View("Error");
    }
}

