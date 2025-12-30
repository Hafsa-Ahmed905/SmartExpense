using FP.Models;

namespace FP.Repositories
{
    public interface ISettingRepository
    {
        Task<List<Setting>> GetAllSettingsAsync();
        Task<List<Setting>> GetAllSettingsAsync(string userId);
        Task AddSettingAsync(Setting setting);
        Task<Setting?> GetSettingByIdAsync(int id);
        Task UpdateSettingAsync(Setting setting);
        Task DeleteSettingAsync(int id);
        Setting? GetSettingByKey(string userId, string key);
        Task UpdateSettingByKeyAsync(string userId, string key, string value);
        UserSettings GetUserSettings(string userId);
    }
}
