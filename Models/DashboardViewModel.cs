namespace FP.Models
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal MonthlyBudget { get; set; }

        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<Budget> Budgets { get; set; } = new();
    }
}
