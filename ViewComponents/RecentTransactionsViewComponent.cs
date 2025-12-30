using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FP.Models;
using FP.Repositories;
using FP.Data;

namespace FP.ViewComponents
{
    [ViewComponent]
    public class RecentTransactionsViewComponent : ViewComponent
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecentTransactionsViewComponent(ITransactionRepository transactionRepo, UserManager<ApplicationUser> userManager)
        {
            _transactionRepo = transactionRepo;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            if (user == null)
                return View(new List<Transaction>());

            var transactions = (await _transactionRepo.GetAllTransactionsAsync(user.Id))
                .Take(count)
                .ToList();

            return View(transactions);
        }
    }
}
