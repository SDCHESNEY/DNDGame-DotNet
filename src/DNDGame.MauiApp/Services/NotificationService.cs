using DNDGame.MauiApp.Interfaces;
using Plugin.LocalNotification;

namespace DNDGame.MauiApp.Services;

public class NotificationService : DNDGame.MauiApp.Interfaces.INotificationService
{
    public async Task<bool> RequestPermissionAsync()
    {
        // Plugin.LocalNotification handles permissions automatically
        // Just return true as the plugin manages permissions internally
        return await Task.FromResult(true);
    }

    public async Task ShowNotificationAsync(string title, string message)
    {
        var notification = new NotificationRequest
        {
            NotificationId = Random.Shared.Next(),
            Title = title,
            Description = message,
            BadgeNumber = 1,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(1) // Show immediately
            }
        };

        await LocalNotificationCenter.Current.Show(notification);
    }

    public async Task ScheduleNotificationAsync(string title, string message, DateTime scheduledTime)
    {
        var notification = new NotificationRequest
        {
            NotificationId = Random.Shared.Next(),
            Title = title,
            Description = message,
            BadgeNumber = 1,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = scheduledTime
            }
        };

        await LocalNotificationCenter.Current.Show(notification);
    }

    public Task CancelNotificationAsync(int notificationId)
    {
        LocalNotificationCenter.Current.Cancel(notificationId);
        return Task.CompletedTask;
    }

    public Task CancelAllNotificationsAsync()
    {
        LocalNotificationCenter.Current.CancelAll();
        return Task.CompletedTask;
    }
}