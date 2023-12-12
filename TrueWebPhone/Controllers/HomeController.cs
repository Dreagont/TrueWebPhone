using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TrueWebPhone.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net.Http.Headers;

namespace Lab7MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("/Error/{code:int}")] // xử lý lỗi 404
    public IActionResult HandleError(int code)
    {
        ViewData["ErrorMessage"] = $"Error occurred. The ErrorCode is: {code}";
        return View("Error");
    }
}

