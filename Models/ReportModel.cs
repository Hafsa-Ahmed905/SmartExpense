using Microsoft.AspNetCore.Mvc;

namespace FP.Models
{
    public class ReportModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
