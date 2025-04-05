namespace InventoryManagementSystem.Services
{
    #region INotificationService interface
    public interface INotificationService
    {
        Task SendNotificationAsync(string message);
    }

    #endregion

    #region NotificationService class
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendNotificationAsync(string message)
        {
            _logger.LogInformation("Notification: {Message}", message);
            return Task.CompletedTask;
        }
    }

    #endregion
}
