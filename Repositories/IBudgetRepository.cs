using FP.Models;

namespace FP.Repositories
{
    public interface IBudgetRepository
    {
        // Async methods (existing)
        Task<List<Budget>> GetAllBudgetsAsync();
        Task<List<Budget>> GetAllBudgetsAsync(string userId);  // Added overload with userId
        Task AddBudgetAsync(Budget budget);
        Task<Budget?> GetBudgetByIdAsync(int id);
        Task UpdateBudgetAsync(Budget budget);
        Task DeleteBudgetAsync(int id);
        Task UpdateBudgetSpentAsync(string userId, string category, decimal amount);
        List<Budget> GetAllBudgets(string userId);
    }
}