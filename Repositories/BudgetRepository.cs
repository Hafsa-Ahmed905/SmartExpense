using Dapper;
using FP.Models;
using Microsoft.Data.SqlClient;

namespace FP.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly IConfiguration _config;

        public BudgetRepository(IConfiguration config)
        {
            _config = config;
        }

        private string Conn => _config.GetConnectionString("DefaultConnection");

        // Async methods
        public async Task<List<Budget>> GetAllBudgetsAsync()
        {
            using var conn = new SqlConnection(Conn);
            var result = await conn.QueryAsync<Budget>("SELECT * FROM Budgets ORDER BY Category");
            return result.AsList();
        }

        public async Task<List<Budget>> GetAllBudgetsAsync(string userId)
        {
            using var conn = new SqlConnection(Conn);
            var result = await conn.QueryAsync<Budget>(
                "SELECT * FROM Budgets WHERE UserId = @UserId ORDER BY Category",
                new { UserId = userId });
            return result.AsList();
        }

        public async Task AddBudgetAsync(Budget budget)
        {
            using var conn = new SqlConnection(Conn);
            string sql = @"INSERT INTO Budgets (Category, Amount, Spent, Month, UserId) 
                           VALUES (@Category, @Amount, @Spent, @Month, @UserId)";
            await conn.ExecuteAsync(sql, budget);
        }

        public async Task<Budget?> GetBudgetByIdAsync(int id)
        {
            using var conn = new SqlConnection(Conn);
            return await conn.QueryFirstOrDefaultAsync<Budget>(
                "SELECT * FROM Budgets WHERE Id = @Id",
                new { Id = id });
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            using var conn = new SqlConnection(Conn);
            string sql = @"UPDATE Budgets 
                           SET Category = @Category, 
                               Amount = @Amount, 
                               Spent = @Spent, 
                               Month = @Month 
                           WHERE Id = @Id";
            await conn.ExecuteAsync(sql, budget);
        }

        public async Task DeleteBudgetAsync(int id)
        {
            using var conn = new SqlConnection(Conn);
            await conn.ExecuteAsync("DELETE FROM Budgets WHERE Id = @Id", new { Id = id });
        }

        public async Task UpdateBudgetSpentAsync(string userId, string category, decimal amount)
        {
            using var conn = new SqlConnection(Conn);
            // Update the Spent amount for budgets matching the category
            await conn.ExecuteAsync(
                @"UPDATE Budgets 
                  SET Spent = ISNULL(Spent, 0) + @Amount 
                  WHERE UserId = @UserId AND Category = @Category 
                  AND MONTH(Month) = MONTH(GETDATE()) AND YEAR(Month) = YEAR(GETDATE())",
                new { UserId = userId, Category = category, Amount = amount });
        }

        // Sync method for Dashboard compatibility
        public List<Budget> GetAllBudgets(string userId)
        {
            using var conn = new SqlConnection(Conn);
            return conn.Query<Budget>(
                "SELECT * FROM Budgets WHERE UserId = @UserId ORDER BY Category",
                new { UserId = userId }).ToList();
        }
    }
}