using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Services;

namespace Homework.TelegramBot.ConsoleApp.BackgroundTasks;

public class TodayBackgroundTask(
    INotificationService notificationService,
    IUserRepository userRepository,
    IToDoRepository toDoRepository)
    : BackgroundTask(TimeSpan.FromDays(1), nameof(TodayBackgroundTask))
{
    protected override async Task Execute(CancellationToken ct)
    {
        var users = await userRepository.GetUsers(ct);

        foreach (var user in users)
        {
            var todayTasks = await toDoRepository.GetActiveWithDeadline(
                user.UserId,
                DateTime.UtcNow.Date,
                DateTime.UtcNow.Date.AddDays(1),
                ct);

            if (todayTasks.Count == 0)
                continue;

            var sb = new StringBuilder("Задачи на сегодня:\n");
            foreach (var task in todayTasks)
            {
                sb.AppendLine($"- {task.Name}");
            }

            await notificationService.ScheduleNotification(
                user.UserId,
                $"Today_{DateOnly.FromDateTime(DateTime.UtcNow)}",
                sb.ToString(),
                DateTime.UtcNow,
                ct);
        }
    }
}
