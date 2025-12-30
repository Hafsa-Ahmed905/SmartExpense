using FP.Models;
using FP.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FP.Data;
using Microsoft.EntityFrameworkCore;

namespace FP.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly IBudgetRepository _budgetRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BudgetController(IBudgetRepository budgetRepo, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _budgetRepo = budgetRepo;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Recalculate spent amounts for current month budgets
            await RecalculateBudgetSpent(user.Id);

            var budgets = await _budgetRepo.GetAllBudgetsAsync(user.Id);
            return View(budgets);
        }

        private async Task RecalculateBudgetSpent(string userId)
        {
            var now = DateTime.Now;
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId &&
                           b.Month.Month == now.Month &&
                           b.Month.Year == now.Year)
                .ToListAsync();

            foreach (var budget in budgets)
            {
                var spent = await _context.Transactions
                    .Where(t => t.UserId == userId &&
                               t.Category == budget.Category &&
                               t.Type.ToLower() == "expense" &&
                               t.Date.Month == now.Month &&
                               t.Date.Year == now.Year)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                budget.Spent = spent;
            }

            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Budget budget)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                budget.UserId = user.Id;
                budget.Spent = 0;
                budget.Month = DateTime.Now;

                await _budgetRepo.AddBudgetAsync(budget);
                TempData["SuccessMessage"] = "Budget created successfully!";
                return RedirectToAction("Index");
            }
            return View(budget);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBudget(string Category, decimal Amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var budget = new Budget
            {
                Category = Category,
                Amount = Amount,
                Spent = 0,
                Month = DateTime.Now,
                UserId = user.Id
            };

            await _budgetRepo.AddBudgetAsync(budget);
            TempData["SuccessMessage"] = "Budget saved successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await _budgetRepo.DeleteBudgetAsync(id);
            TempData["SuccessMessage"] = "Budget deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
