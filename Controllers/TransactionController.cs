using FP.Models;
using FP.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FP.Data;
using Microsoft.Extensions.Configuration;
using FP.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FP.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IBudgetRepository _budgetRepo;
        private readonly IConfiguration _config;
        private readonly INotificationService _notificationService;

        public TransactionController(ITransactionRepository repo, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IBudgetRepository budgetRepo, IConfiguration config, INotificationService notificationService)
        {
            _repo = repo;
            _userManager = userManager;
            _context = context;
            _budgetRepo = budgetRepo;
            _config = config;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> List(string type = null, string category = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            string userId = user.Id;
            var transactions = await _repo.GetAllTransactionsAsync(userId);
            
            // Debug: Check if transactions exist
            if (transactions != null)
            {
                if (transactions.Count == 0)
                {
                    ViewBag.NoTransactions = true;
                }
                else
                {
                    ViewBag.TotalTransactions = transactions.Count;
                }
                
                // Apply filters - more flexible comparison
                if (!string.IsNullOrEmpty(type))
                {
                    transactions = transactions.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                if (!string.IsNullOrEmpty(category))
                {
                    transactions = transactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                if (fromDate.HasValue)
                {
                    transactions = transactions.Where(t => t.Date.Date >= fromDate.Value.Date).ToList();
                }
                
                if (toDate.HasValue)
                {
                    transactions = transactions.Where(t => t.Date.Date <= toDate.Value.Date).ToList();
                }
            }
            
            // Get categories for dropdown
            var settingRepo = new SettingRepository(_config);
            var userSettings = settingRepo.GetUserSettings(userId);
            ViewBag.Categories = userSettings.Categories;
            
            // Pass filter values back to view
            ViewBag.Type = type;
            ViewBag.Category = category;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            
            return View(transactions);
        }

        public async Task<IActionResult> Add()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var model = new Transaction
            {
                Date = DateTime.Now,
                Description = string.Empty
            };
            
            // Get categories for dropdown
            var settingRepo = new SettingRepository(_config);
            var userSettings = settingRepo.GetUserSettings(user.Id);
            ViewBag.Categories = userSettings.Categories;
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Transaction model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Set default values - Description is required in database
            if (string.IsNullOrWhiteSpace(model.Description))
                model.Description = "No description";

            if (model.Date == default)
                model.Date = DateTime.Now;

            // Validate required fields
            if (string.IsNullOrEmpty(model.Type) || string.IsNullOrEmpty(model.Category) || model.Amount <= 0)
            {
                ModelState.AddModelError("", "Please fill in all required fields.");
                return View(model);
            }

            model.UserId = user.Id;

            try
            {
                await _repo.AddTransactionAsync(model);

                string customIcon;
                if (model.Type.ToLower() == "income")
                {
                    customIcon = "💰";
                }
                else
                {
                    customIcon = "💸";
                }
                
                string actionText;
                if (model.Type.ToLower() == "income")
                {
                    actionText = "added to";
                }
                else
                {
                    actionText = "spent on";
                }
                
                await _notificationService.SendCustomNotificationAsync(user.Id, 
                    $"{model.Type}: ${model.Amount:F2} {actionText} {model.Category}", 
                    customIcon);

                if (model.Type.ToLower() == "expense")
                {
                    await _budgetRepo.UpdateBudgetSpentAsync(user.Id, model.Category, model.Amount);
                    
                    var budget = await _context.Budgets
                        .FirstOrDefaultAsync(b => b.UserId == user.Id && 
                                                  b.Category == model.Category &&
                                                  b.Month.Month == DateTime.Now.Month &&
                                                  b.Month.Year == DateTime.Now.Year);
                    
                    if (budget != null)
                    {
                        string enemyIcon;
                        if (budget.PercentageUsed >= 100)
                        {
                            enemyIcon = "😈";
                        }
                        else if (budget.PercentageUsed >= 90)
                        {
                            enemyIcon = "👹";
                        }
                        else
                        {
                            enemyIcon = string.Empty;
                        }
                        await _notificationService.SendBudgetAlertWithCustomIconAsync(user.Id, budget.Category, budget.PercentageUsed, enemyIcon);
                    }
                }

                TempData["SuccessMessage"] = "Transaction added successfully!";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while adding the transaction: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Get transaction before deleting to update budget
            var transaction = await _repo.GetTransactionByIdAsync(id);
            if (transaction != null && transaction.UserId == user.Id)
            {
                await _repo.DeleteTransactionAsync(id);

                // Update budget spent if it was an expense (subtract the amount)
                if (transaction.Type.ToLower() == "expense")
                {
                    var budget = await _context.Budgets
                        .FirstOrDefaultAsync(b => b.UserId == user.Id && 
                                                  b.Category == transaction.Category &&
                                                  b.Month.Month == transaction.Date.Month &&
                                                  b.Month.Year == transaction.Date.Year);
                    if (budget != null)
                    {
                        budget.Spent = Math.Max(0, budget.Spent - transaction.Amount);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction("List");
        }
    }
}
