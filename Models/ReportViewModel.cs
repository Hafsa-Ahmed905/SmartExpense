namespace FP.Models
{
    public class ReportViewModel
    {
        public Dictionary<string, decimal> ExpenseBreakdown { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetSavings { get; set; }
        public List<MonthlyData> MonthlyTrends { get; set; }
        public string LargestExpenseCategory { get; set; }
        public decimal LargestExpensePercentage { get; set; }
        public decimal SavingsRate { get; set; }
        public int CategoriesOverBudget { get; set; }
    }
}