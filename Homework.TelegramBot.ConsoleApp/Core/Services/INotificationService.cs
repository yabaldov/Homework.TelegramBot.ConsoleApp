using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Core.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Создает нотификацию. Если запись с userId и type уже есть, то вернуть false и не добавлять запись, иначе вернуть true.
        /// </summary>
        Task<bool> ScheduleNotification(
            Guid userId,
            string type,
            string text,
            DateTime scheduledAt,
            CancellationToken ct);

        /// <summary>
        /// Возвращает нотификации, у которых IsNotified = false и ScheduledAt &lt;= scheduledBefore.
        /// </summary>
        Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct);

        Task MarkNotified(Guid notificationId, CancellationToken ct);
    }
}
