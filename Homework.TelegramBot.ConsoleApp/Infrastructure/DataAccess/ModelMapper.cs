using Homework.TelegramBot.ConsoleApp.Core.DataAccess.Models;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            return new ToDoUser
            {
                UserId = model.UserId,
                TelegramUserId = model.TelegramUserId,
                TelegramUserName = model.TelegramUserName,
                RegisteredAt = model.RegisteredAt
            };
        }

        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            return new ToDoUserModel
            {
                UserId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                TelegramUserName = entity.TelegramUserName,
                RegisteredAt = entity.RegisteredAt
            };
        }

        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            return new ToDoItem
            {
                Id = model.Id,
                User = MapFromModel(model.User),
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                Deadline = model.Deadline,
                State = model.State,
                StateChangedAt = model.StateChangedAt,
                List = model.List != null ? MapFromModel(model.List) : null
            };
        }

        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            return new ToDoItemModel
            {
                Id = entity.Id,
                UserId = entity.User.UserId,
                ListId = entity.List?.Id,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                Deadline = entity.Deadline,
                State = entity.State,
                StateChangedAt = entity.StateChangedAt
            };
        }

        public static ToDoList MapFromModel(ToDoListModel model)
        {
            return new ToDoList
            {
                Id = model.Id,
                Name = model.Name,
                User = MapFromModel(model.User),
                CreatedAt = model.CreatedAt
            };
        }

        public static ToDoListModel MapToModel(ToDoList entity)
        {
            return new ToDoListModel
            {
                Id = entity.Id,
                Name = entity.Name,
                UserId = entity.User.UserId,
                CreatedAt = entity.CreatedAt
            };
        }

        public static Notification MapFromModel(NotificationModel model)
        {
            return new Notification
            {
                Id = model.Id,
                User = MapFromModel(model.User),
                Type = model.Type,
                Text = model.Text,
                ScheduledAt = model.ScheduledAt,
                IsNotified = model.IsNotified,
                NotifiedAt = model.NotifiedAt
            };
        }

        public static NotificationModel MapToModel(Notification entity)
        {
            return new NotificationModel
            {
                Id = entity.Id,
                UserId = entity.User.UserId,
                Type = entity.Type,
                Text = entity.Text,
                ScheduledAt = entity.ScheduledAt,
                IsNotified = entity.IsNotified,
                NotifiedAt = entity.NotifiedAt
            };
        }
    }
}
