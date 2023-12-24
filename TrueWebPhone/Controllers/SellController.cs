using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrueWebPhone.Models;

namespace TrueWebPhone.Controllers
{
    public class SellController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public SellController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var products = _dbContext.Products.ToList();
            return View(products);
        }
    }
}
