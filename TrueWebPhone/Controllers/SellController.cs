using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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

        public IActionResult AllOrders()
        {
            var orders = _dbContext.Orders.ToList();
            return View(orders);
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

        public IActionResult Delete(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(a => a.Id == id);

            if (order == null)
            {
                return NotFound(); 
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(a => a.Id == id);

            if (order == null)
            {
                return NotFound(); 
            }

            _dbContext.Orders.Remove(order);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Orders));
        }

        public IActionResult Orders(string start, string end)
        {
            IQueryable<Order> query = _dbContext.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                DateTime parsedStart = DateTime.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime parsedEnd = DateTime.ParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                query = query.Where(o => DateTime.ParseExact(o.CreatedDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) >= parsedStart &&
                                          DateTime.ParseExact(o.CreatedDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) <= parsedEnd);
            }

            var orders = query.ToList();
            return View(orders);
        }


    }
}
