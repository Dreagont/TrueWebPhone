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

    public IActionResult Search(string searchString)
    {
        var customers = ct.Customers.ToList();

        if (!string.IsNullOrEmpty(searchString))
        {
            customers = customers.Where(c =>
                c.Name.ToLower().Contains(searchString) ||
                c.Phone.ToLower().Contains(searchString) ||
                c.Address.ToLower().Contains(searchString)
            // Add additional fields you want to search here
            ).ToList();
        }

        return View("~/Views/Home/CustomerList.cshtml", customers);
    }

    [HttpPost]
    [Authorize(Roles = "Admin , Seller")]
    public IActionResult CreateCustomer(string phone, string name, string address)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Customer customer = new Customer(phone, name, address);
                ct.Customers.Add(customer);
                ct.SaveChanges();

                var successResult = new
                {
                    success = true
                };

                return Json(successResult);
            }
            else
            {
                var errorResult = new
                {
                    success = false,
                    errorMessage = "Invalid input. Please check the provided data."
                };

                return Json(errorResult);
            }
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                errorMessage = "An error occurred while processing your request. Please try again later."
            };

            return Json(errorResult);
        }
    }




    [HttpPost]
    [AllowAnonymous]
    public IActionResult HandleCheckout(string phone, string fullname, string address, int paymethod)
    {
        // Log the received phone number for debugging
        System.Diagnostics.Debug.WriteLine($"Received phone: {phone}");

        var existingCustomer = ct.Customers.FirstOrDefault(c => c.Phone == phone);

        if (existingCustomer != null)
        {
            var result = new
            {
                ExistingCustomer = true,
                ExistingName = existingCustomer.Name,
                ExistingAddress = existingCustomer.Address
            };

            return Json(result);
        }
        else
        {
            var result = new
            {
                ExistingCustomer = false
            };

            return Json(result);
        }
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

