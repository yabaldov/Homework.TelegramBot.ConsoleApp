using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Homework.TelegramBot.ConsoleApp.Core.Services;

namespace Homework.TelegramBot.ConsoleApp.BackgroundTasks;

public class NotificationBackgroundTask(
    INotificationService notificationService,
    ITelegramBotClient bot)
    : BackgroundTask(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
{
    protected override async Task Execute(CancellationToken ct)
    {
        var notifications = await notificationService.GetScheduledNotification(DateTime.UtcNow, ct);

        foreach (var notification in notifications)
        {
            await bot.SendMessage(
                notification.User.TelegramUserId,
                notification.Text,
                cancellationToken: ct);

            await notificationService.MarkNotified(notification.Id, ct);
        }
    }
}
