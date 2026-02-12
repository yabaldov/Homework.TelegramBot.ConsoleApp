using System;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Services;

namespace Homework.TelegramBot.ConsoleApp.BackgroundTasks;

public class DeadlineBackgroundTask(
    INotificationService notificationService,
    IUserRepository userRepository,
    IToDoRepository toDoRepository)
    : BackgroundTask(TimeSpan.FromHours(1), nameof(DeadlineBackgroundTask))
{
    protected override async Task Execute(CancellationToken ct)
    {
        var users = await userRepository.GetUsers(ct);

        foreach (var user in users)
        {
            var overdueTasks = await toDoRepository.GetActiveWithDeadline(
                user.UserId,
                DateTime.UtcNow.AddDays(-1).Date,
                DateTime.UtcNow.Date,
                ct);

            foreach (var task in overdueTasks)
            {
                await notificationService.ScheduleNotification(
                    user.UserId,
                    $"Deadline_{task.Id}",
                    $"Ой\\! Вы пропустили дедлайн по задаче {task.Name}",
                    DateTime.UtcNow,
                    ct);
            }
        }
    }
}
