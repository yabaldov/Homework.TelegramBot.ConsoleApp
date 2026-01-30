using System;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess.Dto
{
    public class ToDoUserDto
    {
        public Guid UserId { get; set; }
        public long TelegramUserId { get; set; }
        public string TelegramUserName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }

        public static ToDoUserDto FromEntity(ToDoUser user)
        {
            return new ToDoUserDto
            {
                UserId = user.UserId,
                TelegramUserId = user.TelegramUserId,
                TelegramUserName = user.TelegramUserName,
                RegisteredAt = user.RegisteredAt
            };
        }

        public ToDoUser ToEntity()
        {
            return new ToDoUser(UserId, TelegramUserId, TelegramUserName, RegisteredAt);
        }
    }
}
