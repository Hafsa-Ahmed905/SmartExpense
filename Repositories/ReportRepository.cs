using Dapper;
using FP.Models;
using Microsoft.Data.SqlClient;

namespace FP.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IConfiguration _config;

        public ReportRepository(IConfiguration config)
        {
            _config = config;
        }

        private string Conn => _config.GetConnectionString("DefaultConnection");

        public Dictionary<string, decimal> GetExpenseBreakdown(string userId, int months = 1)
        {
            using var conn = new SqlConnection(Conn);

            var sql = @"SELECT Category, SUM(Amount) as Total
                        FROM Transactions
                        WHERE Type = 'expense'
                        AND UserId = @UserId
                        AND Date >= DATEADD(month, -@Months, GETDATE())
                        GROUP BY Category
                        ORDER BY Total DESC";

            var result = conn.Query<(string Category, decimal Total)>(sql, new { UserId = userId, Months = months });

            return result.ToDictionary(x => x.Category, x => x.Total);
        }

        public List<MonthlyData> GetMonthlyTrends(string userId, int months = 3)
        {
            using var conn = new SqlConnection(Conn);

            var sql = @"SELECT FORMAT(Date, 'yyyy-MM') as Month, 
                       SUM(CASE WHEN Type = 'income' THEN Amount ELSE 0 END) as Income,
                       SUM(CASE WHEN Type = 'expense' THEN Amount ELSE 0 END) as Expenses
                       FROM Transactions
                       WHERE UserId = @UserId
                       AND Date >= DATEADD(month, -@Months, GETDATE())
                       GROUP BY FORMAT(Date, 'yyyy-MM')
                       ORDER BY Month";

            var result = conn.Query<(string Month, decimal Income, decimal Expenses)>(sql, new { UserId = userId, Months = months });

            return result.Select(x => new MonthlyData
            {
                Month = x.Month,
                Income = x.Income,
                Expenses = x.Expenses
            }).ToList();
        }

        public decimal GetTotalIncome(string userId, int months = 1)
        {
            using var conn = new SqlConnection(Conn);
            var result = conn.ExecuteScalar<decimal?>(
                "SELECT SUM(Amount) FROM Transactions WHERE Type = 'income' AND UserId = @UserId AND Date >= DATEADD(month, -@Months, GETDATE())",
                new { UserId = userId, Months = months });
            
            if (result.HasValue)
            {
                return result.Value;
            }
            else
            {
                return 0;
            }
        }

        public decimal GetTotalExpenses(string userId, int months = 1)
        {
            using var conn = new SqlConnection(Conn);
            var result = conn.ExecuteScalar<decimal?>(
                "SELECT SUM(Amount) FROM Transactions WHERE Type = 'expense' AND UserId = @UserId AND Date >= DATEADD(month, -@Months, GETDATE())",
                new { UserId = userId, Months = months });
            
            if (result.HasValue)
            {
                return result.Value;
            }
            else
            {
                return 0;
            }
        }

        public string GetLargestExpenseCategory(string userId, int months = 1)
        {
            using var conn = new SqlConnection(Conn);

            var sql = @"SELECT TOP 1 Category
                        FROM Transactions
                        WHERE Type = 'expense'
                        AND UserId = @UserId
                        AND Date >= DATEADD(month, -@Months, GETDATE())
                        GROUP BY Category
                        ORDER BY SUM(Amount) DESC";

            var result = conn.QueryFirstOrDefault<string>(sql, new { UserId = userId, Months = months });
            if (result != null)
            {
                return result;
            }
            else
            {
                return "None";
            }
        }

        public int GetCategoriesOverBudget(string userId)
        {
            using var conn = new SqlConnection(Conn);

            var sql = @"SELECT COUNT(DISTINCT t.Category)
                        FROM Transactions t
                        JOIN Budgets b ON t.Category = b.Category AND b.UserId = t.UserId
                        WHERE t.Type = 'expense'
                        AND t.UserId = @UserId
                        AND MONTH(t.Date) = MONTH(GETDATE())
                        AND YEAR(t.Date) = YEAR(GETDATE())
                        GROUP BY t.Category, b.Amount
                        HAVING SUM(t.Amount) > b.Amount";

            return conn.QueryFirstOrDefault<int>(sql, new { UserId = userId });
        }
    }
}
