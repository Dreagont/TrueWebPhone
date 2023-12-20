using Microsoft.AspNetCore.Mvc;

namespace TrueWebPhone.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
