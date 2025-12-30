using Dapper;
using FP.Models;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace FP.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly IConfiguration _config;

        public SettingRepository(IConfiguration config)
        {
            _config = config;
        }

        private string Conn => _config.GetConnectionString("DefaultConnection");

        
        public async Task<List<Setting>> GetAllSettingsAsync()
        {
            using var conn = new SqlConnection(Conn);
            var result = await conn.QueryAsync<Setting>("SELECT * FROM Settings ORDER BY SettingKey");
            return result.AsList();
        }

        public async Task<List<Setting>> GetAllSettingsAsync(string userId)
        {
            using var conn = new SqlConnection(Conn);
            var result = await conn.QueryAsync<Setting>(
                "SELECT * FROM Settings WHERE UserId = @UserId ORDER BY SettingKey",
                new { UserId = userId });
            return result.AsList();
        }

        public async Task AddSettingAsync(Setting setting)
        {
            using var conn = new SqlConnection(Conn);
            var sql = @"INSERT INTO Settings (UserId, SettingKey, SettingValue, FullName, Email, DateFormat) 
                       VALUES (@UserId, @SettingKey, @SettingValue, @FullName, @Email, @DateFormat)";
            await conn.ExecuteAsync(sql, setting);
        }

        public async Task<Setting?> GetSettingByIdAsync(int id)
        {
            using var conn = new SqlConnection(Conn);
            return await conn.QueryFirstOrDefaultAsync<Setting>(
                "SELECT * FROM Settings WHERE Id = @Id", new { Id = id });
        }

        public async Task UpdateSettingAsync(Setting setting)
        {
            using var conn = new SqlConnection(Conn);
            var sql = @"UPDATE Settings SET 
                       SettingValue = @SettingValue, FullName = @FullName, Email = @Email, DateFormat = @DateFormat 
                       WHERE Id = @Id";
            await conn.ExecuteAsync(sql, setting);
        }

        public async Task DeleteSettingAsync(int id)
        {
            using var conn = new SqlConnection(Conn);
            await conn.ExecuteAsync("DELETE FROM Settings WHERE Id = @Id", new { Id = id });
        }

        public Setting? GetSettingByKey(string userId, string key)
        {
            using var conn = new SqlConnection(Conn);
            return conn.QueryFirstOrDefault<Setting>(
                "SELECT * FROM Settings WHERE UserId = @UserId AND SettingKey = @SettingKey",
                new { UserId = userId, SettingKey = key });
        }

        public async Task UpdateSettingByKeyAsync(string userId, string key, string value)
        {
            using var conn = new SqlConnection(Conn);
            
            // Check if setting exists
            var existing = await conn.QueryFirstOrDefaultAsync<Setting>(
                "SELECT * FROM Settings WHERE UserId = @UserId AND SettingKey = @SettingKey",
                new { UserId = userId, SettingKey = key });
            
            if (existing != null)
            {
                // Update existing setting
                await conn.ExecuteAsync(
                    "UPDATE Settings SET SettingValue = @Value WHERE UserId = @UserId AND SettingKey = @SettingKey",
                    new { UserId = userId, SettingKey = key, Value = value });
            }
            else
            {
                // Create new setting - try to get user info from existing settings or use defaults
                var existingSettings = await GetAllSettingsAsync(userId);
                var firstSetting = existingSettings.FirstOrDefault();
                
                var newSetting = new Setting
                {
                    UserId = userId,
                    SettingKey = key,
                    SettingValue = value,
                    FullName = firstSetting?.FullName != null ? firstSetting.FullName : "",
                    Email = firstSetting?.Email != null ? firstSetting.Email : "",
                    DateFormat = firstSetting?.DateFormat != null ? firstSetting.DateFormat : "MM/dd/yyyy"
                };
                await AddSettingAsync(newSetting);
            }
        }

        public UserSettings GetUserSettings(string userId)
        {
            var settings = new UserSettings
            {
                Categories = new List<string> { "Food & Dining", "Transportation", "Shopping", "Entertainment", "Utilities", "Salary", "Freelance" }
            };

            using var conn = new SqlConnection(Conn);
            var userSettings = conn.Query<Setting>(
                "SELECT * FROM Settings WHERE UserId = @UserId",
                new { UserId = userId }).ToList();

            foreach (var setting in userSettings)
            {
                switch (setting.SettingKey)
                {
                    case "Categories":
                        if (!string.IsNullOrEmpty(setting.SettingValue))
                        {
                            var categories = JsonSerializer.Deserialize<List<string>>(setting.SettingValue);
                            if (categories != null)
                            {
                                settings.Categories = categories;
                            }
                        }
                        break;
                    case "Currency":
                        if (setting.SettingValue != null)
                        {
                            settings.Currency = setting.SettingValue;
                        }
                        else
                        {
                            settings.Currency = "USD";
                        }
                        break;
                    case "DateFormat":
                        if (setting.SettingValue != null)
                        {
                            settings.DateFormat = setting.SettingValue;
                        }
                        else
                        {
                            settings.DateFormat = "MM/dd/yyyy";
                        }
                        break;
                    case "Theme":
                        if (setting.SettingValue != null)
                        {
                            settings.Theme = setting.SettingValue;
                        }
                        else
                        {
                            settings.Theme = "light";
                        }
                        break;
                }
            }

            return settings;
        }
    }
}
