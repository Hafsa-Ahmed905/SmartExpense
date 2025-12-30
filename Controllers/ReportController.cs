using FP.Models;
using FP.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FP.Data;

namespace FP.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportRepository _reportRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(IReportRepository reportRepo, UserManager<ApplicationUser> userManager)
        {
            _reportRepo = reportRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string period = "month")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            string userId = user.Id;
            int months;
            if (period == "3months")
            {
                months = 3;
            }
            else
            {
                months = 1;
            }

            var totalIncome = _reportRepo.GetTotalIncome(userId, months);
            var totalExpenses = _reportRepo.GetTotalExpenses(userId, months);

            var viewModel = new ReportViewModel
            {
                ExpenseBreakdown = _reportRepo.GetExpenseBreakdown(userId, months),
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetSavings = totalIncome - totalExpenses,
                MonthlyTrends = _reportRepo.GetMonthlyTrends(userId, months),
                LargestExpenseCategory = _reportRepo.GetLargestExpenseCategory(userId, months),
                CategoriesOverBudget = _reportRepo.GetCategoriesOverBudget(userId)
            };

            if (totalExpenses > 0 && viewModel.ExpenseBreakdown.Count > 0)
            {
                var largestAmount = viewModel.ExpenseBreakdown.Values.Max();
                viewModel.LargestExpensePercentage = (largestAmount / totalExpenses) * 100;
            }

            if (totalIncome > 0)
            {
                viewModel.SavingsRate = (viewModel.NetSavings / totalIncome) * 100;
            }

            ViewBag.Period = period;
            return View(viewModel);
        }
    }
}