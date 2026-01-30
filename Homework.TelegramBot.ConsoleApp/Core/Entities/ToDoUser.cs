using System;

namespace Homework.TelegramBot.ConsoleApp.Core.Entities
{
    public class ToDoUser
    {
        public Guid UserId { get; }
        public long TelegramUserId { get; }
        public string TelegramUserName { get; }
        public DateTime RegisteredAt { get; }

        public ToDoUser(long telegramUserId, string telegramUserName)
        {
            UserId = Guid.NewGuid();
            TelegramUserId = telegramUserId;
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.UtcNow;
        }

        // Конструктор для восстановления из файла
        public ToDoUser(Guid userId, long telegramUserId, string telegramUserName, DateTime registeredAt)
        {
            UserId = userId;
            TelegramUserId = telegramUserId;
            TelegramUserName = telegramUserName;
            RegisteredAt = registeredAt;
        }
    }
}
