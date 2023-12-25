using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrueWebPhone.Models;

namespace TrueWebPhone.Controllers
{
    [Produces("application/json")]

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

        public IActionResult Orders()
        {
            var order = _dbContext.Orders.ToList();
            return View(order);
        }

        public IActionResult Details(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(a => a.Id == id);
            var customer = _dbContext.Customers.FirstOrDefault(a => a.Id == order.CustomerId);
            var seller = _dbContext.Accounts.FirstOrDefault(a => a.Id == order.StaffId);

            var details = new
            {
                orderNumber = order.OrderNumber,
                change = order.Change,
                cash = order.Cash,
                total = order.Total,
                payment = order.PaymentMethod,
                date = order.CreatedDate,
                cusName = customer.Name,
                phone = customer.Phone,
                sellerName = seller.Name,
            };

            return View("Details", details); 
        }

    }
}
