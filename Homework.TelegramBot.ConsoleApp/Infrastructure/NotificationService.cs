using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess.Models;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Services;
using Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess;
using LinqToDB;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure
{
    public class NotificationService : INotificationService
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public NotificationService(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<bool> ScheduleNotification(
            Guid userId,
            string type,
            string text,
            DateTime scheduledAt,
            CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            var exists = await db.Notifications
                .AnyAsync(n => n.UserId == userId && n.Type == type, ct);

            if (exists)
                return false;

            var model = new NotificationModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Text = text,
                ScheduledAt = scheduledAt,
                IsNotified = false,
                NotifiedAt = null
            };

            await db.InsertAsync(model, token: ct);
            return true;
        }

        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            var models = await db.Notifications
                .LoadWith(n => n.User)
                .Where(n => !n.IsNotified && n.ScheduledAt <= scheduledBefore)
                .ToListAsync(ct);

            return models.Select(DataAccess.ModelMapper.MapFromModel).ToList();
        }

        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            await db.Notifications
                .Where(n => n.Id == notificationId)
                .Set(n => n.IsNotified, true)
                .Set(n => n.NotifiedAt, DateTime.UtcNow)
                .UpdateAsync(ct);
        }
    }
}
