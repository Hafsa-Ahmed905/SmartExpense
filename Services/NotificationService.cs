using FP.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FP.Services
{
    public interface INotificationService
    {
        Task SendNotificationToUserAsync(string userId, string message);
        Task SendBudgetAlertAsync(string userId, string category, decimal percentage);
        Task SendTransactionNotificationAsync(string userId, string type, decimal amount, string category);
        Task SendContactSubmissionNotificationAsync(string name, string email, string subject);
        Task SendCustomNotificationAsync(string userId, string message, string iconEmoji = null, string imageUrl = null);
        Task SendBudgetAlertWithCustomIconAsync(string userId, string category, decimal percentage, string customIcon = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationToUserAsync(string userId, string message)
        {
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
                _logger.LogInformation($"Notification sent to user {userId}: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification to user {userId}: {ex.Message}");
            }
        }

        public async Task SendBudgetAlertAsync(string userId, string category, decimal percentage)
        {
            string message;
            if (percentage >= 100)
            {
                message = $"🚨🚨 CRITICAL BUDGET ALERT! You've EXCEEDED your {category} budget by {(percentage - 100):F0}%! Total spending: {percentage:F0}% of budget";
            }
            else if (percentage >= 90)
            {
                message = $"⚠️ URGENT: You've used {percentage:F0}% of your {category} budget! Only {(100 - percentage):F0}% remaining";
            }
            else if (percentage >= 75)
            {
                message = $"⚠️ Budget Warning: You've used {percentage:F0}% of your {category} budget";
            }
            else
            {
                message = $"📊 Budget Update: {percentage:F0}% of {category} budget used";
            }

            await SendNotificationToUserAsync(userId, message);
        }

        public async Task SendTransactionNotificationAsync(string userId, string type, decimal amount, string category)
        {
            string message = type.ToLower() == "income" 
                ? $"💰 Income: ${amount:F2} added to {category}"
                : $"💸 Expense: ${amount:F2} spent on {category}";

            await SendNotificationToUserAsync(userId, message);
        }

        public async Task SendContactSubmissionNotificationAsync(string name, string email, string subject)
        {
            // Send to admin users (you could modify this to send to specific admin users)
            string message = $"📧 New Contact Form: {name} ({email}) - {subject}";
            
            // For now, send to all connected users (you might want to filter for admin users only)
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
                _logger.LogInformation($"Contact submission notification sent: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending contact notification: {ex.Message}");
            }
        }

        public async Task SendCustomNotificationAsync(string userId, string message, string iconEmoji = null, string imageUrl = null)
        {
            string fullMessage = message;
            
            // Add custom icon to message if provided
            if (!string.IsNullOrEmpty(iconEmoji))
            {
                fullMessage = $"{iconEmoji} {message}";
            }
            else if (!string.IsNullOrEmpty(imageUrl))
            {
                fullMessage = $"📷 {message}";
            }

            await SendNotificationToUserAsync(userId, fullMessage);
        }

        public async Task SendBudgetAlertWithCustomIconAsync(string userId, string category, decimal percentage, string customIcon = null)
        {
            string message;
            string iconPrefix = customIcon ?? "⚠️";
            
            if (percentage >= 100)
            {
                message = $"{iconPrefix}{iconPrefix} CRITICAL BUDGET ALERT! You've EXCEEDED your {category} budget by {(percentage - 100):F0}%! Total spending: {percentage:F0}% of budget";
            }
            else if (percentage >= 90)
            {
                message = $"{iconPrefix} URGENT: You've used {percentage:F0}% of your {category} budget! Only {(100 - percentage):F0}% remaining";
            }
            else if (percentage >= 75)
            {
                message = $"{iconPrefix} Budget Warning: You've used {percentage:F0}% of your {category} budget";
            }
            else
            {
                message = $"📊 Budget Update: {percentage:F0}% of {category} budget used";
            }

            await SendNotificationToUserAsync(userId, message);
        }
    }
}
