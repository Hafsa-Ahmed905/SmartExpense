using Dapper;
using FP.Models;
using Microsoft.Data.SqlClient;

namespace FP.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IConfiguration _config;

        public TransactionRepository(IConfiguration config)
        {
            _config = config;
        }

        private string Conn => _config.GetConnectionString("DefaultConnection");

        // Async methods
        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            using var conn = new SqlConnection(Conn);
            var result = await conn.QueryAsync<Transaction>("SELECT * FROM Transactions ORDER BY Date DESC");
            return result.AsList();
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync(string userId)
        {
            using var conn = new SqlConnection(Conn);
            var result = await conn.QueryAsync<Transaction>(
                "SELECT * FROM Transactions WHERE UserId = @UserId ORDER BY Date DESC",
                new { UserId = userId });
            return result.AsList();
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            using var conn = new SqlConnection(Conn);
            var sql = @"INSERT INTO Transactions (Amount, Type, Category, Date, Description, UserId) 
                       VALUES (@Amount, @Type, @Category, @Date, @Description, @UserId)";
            await conn.ExecuteAsync(sql, transaction);
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            using var conn = new SqlConnection(Conn);
            return await conn.QueryFirstOrDefaultAsync<Transaction>(
                "SELECT * FROM Transactions WHERE Id = @Id", new { Id = id });
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            using var conn = new SqlConnection(Conn);
            var sql = @"UPDATE Transactions SET 
                       Amount = @Amount, Type = @Type, Category = @Category, 
                       Date = @Date, Description = @Description 
                       WHERE Id = @Id";
            await conn.ExecuteAsync(sql, transaction);
        }

        public async Task DeleteTransactionAsync(int id)
        {
            using var conn = new SqlConnection(Conn);
            await conn.ExecuteAsync("DELETE FROM Transactions WHERE Id = @Id", new { Id = id });
        }

        // Sync method for Dashboard compatibility
        public List<Transaction> GetAllTransactions(string userId)
        {
            using var conn = new SqlConnection(Conn);
            return conn.Query<Transaction>(
                "SELECT * FROM Transactions WHERE UserId = @UserId ORDER BY Date DESC",
                new { UserId = userId }).ToList();
        }

        // Dashboard methods
        public decimal GetTotalBalance(string userId)
        {
            using var conn = new SqlConnection(Conn);
            var incomeResult = conn.ExecuteScalar<decimal?>(
                "SELECT SUM(Amount) FROM Transactions WHERE Type = 'income' AND UserId = @UserId",
                new { UserId = userId });
            
            decimal income;
            if (incomeResult.HasValue)
            {
                income = incomeResult.Value;
            }
            else
            {
                income = 0;
            }

            var expensesResult = conn.ExecuteScalar<decimal?>(
                "SELECT SUM(Amount) FROM Transactions WHERE Type = 'expense' AND UserId = @UserId",
                new { UserId = userId });
            
            decimal expenses;
            if (expensesResult.HasValue)
            {
                expenses = expensesResult.Value;
            }
            else
            {
                expenses = 0;
            }

            return income - expenses;
        }

        public decimal GetMonthlyIncome(string userId)
        {
            using var conn = new SqlConnection(Conn);
            var result = conn.ExecuteScalar<decimal?>(
                "SELECT SUM(Amount) FROM Transactions WHERE Type = 'income' AND UserId = @UserId AND MONTH(Date) = MONTH(GETDATE()) AND YEAR(Date) = YEAR(GETDATE())",
                new { UserId = userId });
            
            if (result.HasValue)
            {
                return result.Value;
            }
            else
            {
                return 0;
            }
        }

        public decimal GetMonthlyExpenses(string userId)
        {
            using var conn = new SqlConnection(Conn);
            var result = conn.ExecuteScalar<decimal?>(
                "SELECT SUM(Amount) FROM Transactions WHERE Type = 'expense' AND UserId = @UserId AND MONTH(Date) = MONTH(GETDATE()) AND YEAR(Date) = YEAR(GETDATE())",
                new { UserId = userId });
            
            if (result.HasValue)
            {
                return result.Value;
            }
            else
            {
                return 0;
            }
        }
    }
}
