using Microsoft.AspNetCore.Mvc;

namespace FP.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
