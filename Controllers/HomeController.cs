using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FP.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "This is SmartExpense - your personal budgeting assistant.";
            return View();
        }

        public IActionResult Features()
        {
            var features = new List<string>()
            {
                "Add & Track Transactions",
                "Set Monthly Budget",
                "View Spending Reports",
                "Dashboard Overview",
                "User Authentication & Security"
            };
            return View(features);
        }

        public IActionResult Contact(string message = "")
        {
            ViewBag.QueryMessage = message;
            return View();
        }

        [HttpPost]
        public IActionResult SubmitContact(string Name, string Email, string Subject, string Message)
        {
            TempData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you soon.";
            return RedirectToAction("Contact");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
