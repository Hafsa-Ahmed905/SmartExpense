using Microsoft.AspNetCore.Mvc;
using FP.Models;
using FP.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FP.Data;

namespace FP.Controllers
{
    [Authorize]
    public class SettingController : Controller
    {
        private readonly ISettingRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingController(ISettingRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Load existing user settings
            var model = _repository.GetUserSettings(user.Id);
            
            // Set user profile data
            if (user.FullName != null)
            {
                model.FullName = user.FullName;
            }
            else
            {
                model.FullName = string.Empty;
            }
            
            if (user.Email != null)
            {
                model.Email = user.Email;
            }
            else
            {
                model.Email = string.Empty;
            }

            // Load dark mode and other preferences
            var darkModeSetting = _repository.GetSettingByKey(user.Id, "DarkMode");
            if (darkModeSetting != null && bool.TryParse(darkModeSetting.SettingValue, out bool darkMode))
            {
                model.DarkMode = darkMode;
            }

            var showBudgetAlerts = _repository.GetSettingByKey(user.Id, "ShowBudgetAlerts");
            if (showBudgetAlerts != null && bool.TryParse(showBudgetAlerts.SettingValue, out bool alerts))
            {
                model.ShowBudgetAlerts = alerts;
            }

            var emailNotifications = _repository.GetSettingByKey(user.Id, "EmailNotifications");
            if (emailNotifications != null && bool.TryParse(emailNotifications.SettingValue, out bool emailNotif))
            {
                model.EmailNotifications = emailNotif;
            }

            var weeklySummary = _repository.GetSettingByKey(user.Id, "WeeklySummary");
            if (weeklySummary != null && bool.TryParse(weeklySummary.SettingValue, out bool weekly))
            {
                model.WeeklySummary = weekly;
            }

            var autoLogout = _repository.GetSettingByKey(user.Id, "AutoLogout");
            if (autoLogout != null && bool.TryParse(autoLogout.SettingValue, out bool auto))
            {
                model.AutoLogout = auto;
            }

            var dateFormat = _repository.GetSettingByKey(user.Id, "DateFormat");
            if (dateFormat != null)
            {
                model.DateFormat = dateFormat.SettingValue;
            }

            return View("~/Views/Settings/Index.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserSettings model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Update user profile in Identity
            if (!string.IsNullOrEmpty(model.FullName))
            {
                var nameParts = model.FullName.Split(' ');
                if (nameParts.Length >= 2)
                {
                    user.FirstName = nameParts[0];
                    user.LastName = string.Join(" ", nameParts.Skip(1));
                }
                else if (nameParts.Length == 1)
                {
                    user.FirstName = nameParts[0];
                    user.LastName = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                user.Email = model.Email;
                user.UserName = model.Email;
            }

            await _userManager.UpdateAsync(user);

            // Update user settings in database
            await _repository.UpdateSettingByKeyAsync(user.Id, "FullName", model.FullName);
            await _repository.UpdateSettingByKeyAsync(user.Id, "Email", model.Email);

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SavePreferences(UserSettings model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Get or create settings
            var dateFormatSetting = _repository.GetSettingByKey(user.Id, "DateFormat");
            if (dateFormatSetting == null)
            {
                var newSetting = new Setting
                {
                    UserId = user.Id,
                    SettingKey = "DateFormat",
                    SettingValue = model.DateFormat,
                    FullName = user.FullName,
                    Email = user.Email != null ? user.Email : string.Empty,
                    DateFormat = model.DateFormat
                };
                await _repository.AddSettingAsync(newSetting);
            }
            else
            {
                await _repository.UpdateSettingByKeyAsync(user.Id, "DateFormat", model.DateFormat);
            }

            // Update or create DarkMode setting
            var darkModeSetting = _repository.GetSettingByKey(user.Id, "DarkMode");
            if (darkModeSetting == null)
            {
                var newSetting = new Setting
                {
                    UserId = user.Id,
                    SettingKey = "DarkMode",
                    SettingValue = model.DarkMode.ToString(),
                    FullName = user.FullName,
                    Email = user.Email != null ? user.Email : string.Empty,
                    DateFormat = model.DateFormat
                };
                await _repository.AddSettingAsync(newSetting);
            }
            else
            {
                await _repository.UpdateSettingByKeyAsync(user.Id, "DarkMode", model.DarkMode.ToString());
            }

            // Update other preferences
            await UpdateOrCreateSetting(user.Id, "ShowBudgetAlerts", model.ShowBudgetAlerts.ToString(), user);
            await UpdateOrCreateSetting(user.Id, "EmailNotifications", model.EmailNotifications.ToString(), user);
            await UpdateOrCreateSetting(user.Id, "WeeklySummary", model.WeeklySummary.ToString(), user);
            await UpdateOrCreateSetting(user.Id, "AutoLogout", model.AutoLogout.ToString(), user);

            TempData["Message"] = "Preferences saved successfully!";
            return RedirectToAction("Index");
        }

        private async Task UpdateOrCreateSetting(string userId, string key, string value, ApplicationUser user)
        {
            var setting = _repository.GetSettingByKey(userId, key);
            if (setting == null)
            {
                var newSetting = new Setting
                {
                    UserId = userId,
                    SettingKey = key,
                    SettingValue = value,
                    FullName = user.FullName,
                    Email = user.Email != null ? user.Email : string.Empty,
                    DateFormat = "MM/dd/yyyy"
                };
                await _repository.AddSettingAsync(newSetting);
            }
            else
            {
                await _repository.UpdateSettingByKeyAsync(userId, key, value);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(string newCategory)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!string.IsNullOrEmpty(newCategory))
            {
                // Get current categories
                var userSettings = _repository.GetUserSettings(user.Id);
                var categories = userSettings.Categories != null ? userSettings.Categories : new List<string>();
                
                // Add new category if it doesn't already exist
                if (!categories.Contains(newCategory))
                {
                    categories.Add(newCategory);
                    
                    // Save updated categories list as JSON
                    var categoriesJson = System.Text.Json.JsonSerializer.Serialize(categories);
                    await _repository.UpdateSettingByKeyAsync(user.Id, "Categories", categoriesJson);
                }
            }

            TempData["Message"] = $"Category '{newCategory}' added successfully!";
            return RedirectToAction("Index");
        }
    }
}
