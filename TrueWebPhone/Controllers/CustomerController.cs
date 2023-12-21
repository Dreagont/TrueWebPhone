﻿using Microsoft.AspNetCore.Mvc;

namespace TrueWebPhone.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Search(string searchString)
        {
            var customers = _context.Customers.ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c =>
                    c.Name.Contains(searchString) ||
                    c.Phone.Contains(searchString) ||
                    c.Address.Contains(searchString)
                // Add additional fields you want to search here
                ).ToList();
            }

            return View("~/Views/Home/CustomerList.cshtml", customers);
        }
    }
}
