using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Notification;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Music_Synchronizer.Services;

public class NotificationService {
    private static NotificationService _instance;
    private WindowNotificationManager _notificationMessageManager;

    public static void Initialize(Window window) {
        _instance = new();
        _instance._notificationMessageManager = new WindowNotificationManager(window);
        _instance._notificationMessageManager.Position = NotificationPosition.TopLeft;
        _instance._notificationMessageManager.MaxItems = 3;
    }

    public static void Notify(string title, string message, NotificationType notificationType) {
        var notification = new Notification(
            title,
            message,
            notificationType,
            TimeSpan.FromSeconds(7) // Duration before it automatically closes
        );
        _instance._notificationMessageManager.Show(notification);
    }
}