using Microsoft.AspNetCore.Mvc;

namespace FP.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
