using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TrueWebPhone.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TrueWebPhone.Controllers
{
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

        public IActionResult DeTails(int id)
        {
            if (id == null) return RedirectToAction("Index");
            Customer customer = ct.Customers.Where(x => x.Id == id).FirstOrDefault();

            return View(customer);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var customer = ct.Customers.FirstOrDefault(p => p.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
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

        [AllowAnonymous]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = ct.Customers.FirstOrDefault(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View("Delete", customer);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var customer = ct.Customers.FirstOrDefault(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            ct.Customers.Remove(customer);
            ct.SaveChanges();

            return RedirectToAction(nameof(CustomerList));
        }

        [HttpPost]
        public ActionResult CreateBill([FromBody] BillRequestModel requestData)
        {
            try
            {
                var cartItems = requestData.CartItems;
                var customerPayment = requestData.CustomerPayment;
                var customerPhone = requestData.CustomerPhone;
                var paymentMethod = requestData.PaymentMethod;

                var currentUsername = User.Identity.Name;

                Account staff = ct.Accounts.FirstOrDefault(a => a.Username == currentUsername);

                Customer customer = ct.Customers.FirstOrDefault(a => a.Phone == customerPhone);

                if (customer == null)
                {
                    return Json(new { success = false, errorMessage = "Customer not found." });
                }

                decimal calculatedTotalAmount = CalculateTotalAmount(cartItems);

                var order = new Order
                {
                    StaffId = staff?.Id ?? 0,
                    Cash = customerPayment,
                    CustomerId = customer.Id,
                    Change = customerPayment - cartItems.Sum(item => item.TotalPrice),
                    Total = cartItems.Sum(item => item.TotalPrice),
                    Quantity = cartItems.Sum(item => item.Quantity),
                    PaymentMethod = paymentMethod,
                    CreatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    OrderNumber = GenerateRandomOrderNumber()
                };

                ct.Orders.Add(order);
                ct.SaveChanges();

                try
                {
                    foreach (var cartItem in cartItems)
                    {
                        var productOrder = new ProductOrder
                        {
                            OrderId = order.Id,
                            ProductId = cartItem.ProductId,
                            ProductQuantity = cartItem.Quantity
                        };
                        var product = ct.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);

                        if (product != null)
                        {
                            product.IsSelled = true;
                        }

                        ct.ProductOrders.Add(productOrder);
                    }

                    ct.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error saving product orders to the database: " + ex.Message);
                    throw; // Rethrow the exception to see it in the console or logs
                }

                var invoiceDetails = new
                {
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    CustomerAddress = customer.Address,
                    StaffName = staff?.Name ?? "N/A", // Replace "N/A" with a default value if staff is not found
                    OrderDetails = cartItems.Select(item => new
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice
                    })
                };

                return Json(new
                {
                    success = true,
                    message = "Bill created successfully",
                    totalAmount = calculatedTotalAmount,
                    paymentMethod,
                    customerPhone,
                    cartItems,
                    invoiceDetails

                });




            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = "An error occurred while processing your request. Please try again later." });
            }
        }



        private decimal CalculateTotalAmount(List<CartItem> cartItems)
        {
            return cartItems.Sum(item => item.TotalPrice);
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
        private string GenerateRandomOrderNumber()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var orderNumber = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return orderNumber;
        }
    }

    public class BillRequestModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal CustomerPayment { get; set; }
        public string CustomerPhone { get; set; }
        public string PaymentMethod { get; set; }

    }
}

