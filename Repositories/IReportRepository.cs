using FP.Models;

namespace FP.Repositories
{
    public interface IReportRepository
    {
        Dictionary<string, decimal> GetExpenseBreakdown(string userId, int months = 1);
        List<MonthlyData> GetMonthlyTrends(string userId, int months = 3);
        decimal GetTotalIncome(string userId, int months = 1);
        decimal GetTotalExpenses(string userId, int months = 1);
        string GetLargestExpenseCategory(string userId, int months = 1);
        int GetCategoriesOverBudget(string userId);
    }
}