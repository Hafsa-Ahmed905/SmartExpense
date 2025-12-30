using System.ComponentModel.DataAnnotations;

namespace FP.Models
{
    public class Setting
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DateFormat { get; set; } = string.Empty;
    }

    public class UserSettings
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string DateFormat { get; set; } = "MM/dd/yyyy";
        public bool ShowBudgetAlerts { get; set; } = true;
        public bool DarkMode { get; set; } = false;
        public bool AutoLogout { get; set; } = false;
        public List<string> Categories { get; set; } = new List<string>();
        public string Currency { get; set; } = "USD";
        public string Theme { get; set; } = "light";
        public bool EmailNotifications { get; set; } = true;
        public bool WeeklySummary { get; set; } = true;
    }
}
