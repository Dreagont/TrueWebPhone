using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrueWebPhone.Models;

namespace TrueWebPhone.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext ct;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext ct, IWebHostEnvironment webHostEnvironment)
        {
            this.ct = ct;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var products = ct.Products.ToList();

            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product model)
        {
            if (model.ProductImage != null)
            {
                // Save the file to your chosen storage (e.g., local storage)
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProductImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProductImage.CopyToAsync(fileStream);
                }

                // Update the Product entity with the file path or relevant information
                model.ImagePath = uniqueFileName;

                // Save the Product entity to the database
                ct.Products.Add(model);
                await ct.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("ProductImage", "Please choose a file to upload.");
            }
            return View(model);
            }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            Product product = ct.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Product updatedProduct)
        {
                Product existingProduct = ct.Products.Find(updatedProduct.Id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.ProductName = updatedProduct.ProductName;
                existingProduct.IsSelled = updatedProduct.IsSelled;
                existingProduct.Barcode = updatedProduct.Barcode;
                existingProduct.ImportPrice = updatedProduct.ImportPrice;
                existingProduct.RetailPrice = updatedProduct.RetailPrice;
                existingProduct.Category = updatedProduct.Category;
                existingProduct.CreationDate = updatedProduct.CreationDate;
                existingProduct.Quantity = updatedProduct.Quantity;

               
                ct.SaveChanges();

                return RedirectToAction("Index");
            
        }



        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var product = ct.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            Product product = ct.Products.Find(id);

            ct.Products.Remove(product);
            ct.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeTails(int id)
        {
            if (id == null) return RedirectToAction("Index");
            Product product = ct.Products.Where(x => x.Id == id).FirstOrDefault();

            return View(product);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = ct.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpGet]
        public IActionResult Search(string searchString)
        {
            var products = ct.Products.ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.ToLower(); // Convert search string to lowercase

                decimal price;
                bool isNumeric = decimal.TryParse(searchLower, out price); // Check if the entered search string is a numeric value

                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(searchLower) ||
                    p.Barcode.ToLower().Contains(searchLower) ||
                    p.Category.ToLower().Contains(searchLower) ||
                    (isNumeric && p.ImportPrice == price) || // Check if ImportPrice matches the entered numeric value
                    (isNumeric && p.RetailPrice == price)   // Check if RetailPrice matches the entered numeric value
                                                            // Add additional fields you want to search here
                ).ToList();
            }

            return View("Index", products);
        }

    }
}

