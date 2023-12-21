using Microsoft.AspNetCore.Mvc;
using TrueWebPhone.Models;

namespace TrueWebPhone.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext ct;
        public ProductController(ApplicationDbContext ct)
        {
            this.ct = ct;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var products = ct.Products.ToList();

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product p)
        {
            if (ModelState.IsValid)
            {
                // Initialize related entities if needed
                p.ProductOrders = new List<ProductOrder>();

                ct.Products.Add(p);
                ct.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
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
        public IActionResult Edit(Product updatedProduct)
        {
            if (ModelState.IsValid)
            {
                Product existingProduct = ct.Products.Find(updatedProduct.Id);

                if (existingProduct == null)
                {
                    return NotFound();
                }
                existingProduct.ProductName = updatedProduct.ProductName;
                existingProduct.Barcode = updatedProduct.Barcode;
                existingProduct.ImportPrice = updatedProduct.ImportPrice;
                existingProduct.RetailPrice = updatedProduct.RetailPrice;
                existingProduct.Category = updatedProduct.Category;
                existingProduct.CreationDate = updatedProduct.CreationDate;
                ct.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(updatedProduct);
        }


        [HttpGet]
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

    }
}

