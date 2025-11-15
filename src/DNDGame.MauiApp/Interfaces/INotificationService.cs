using Plugin.LocalNotification;

namespace DNDGame.MauiApp.Interfaces;

public interface INotificationService
{
    Task<bool> RequestPermissionAsync();
    Task ShowNotificationAsync(string title, string message);
    Task ScheduleNotificationAsync(string title, string message, DateTime scheduledTime);
    Task CancelNotificationAsync(int notificationId);
    Task CancelAllNotificationsAsync();
}