using Microsoft.AspNetCore.Mvc;

namespace FP.Models
{
    public class User : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
