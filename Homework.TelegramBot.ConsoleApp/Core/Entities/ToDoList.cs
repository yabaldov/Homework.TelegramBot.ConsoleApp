using System;

namespace Homework.TelegramBot.ConsoleApp.Core.Entities
{
    public class ToDoList
    {
        public Guid Id { get; }
        public string Name { get; }
        public ToDoUser User { get; }
        public DateTime CreatedAt { get; }

        public ToDoList(ToDoUser user, string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            User = user;
            CreatedAt = DateTime.UtcNow;
        }

        // Для файла
        public ToDoList(Guid id, string name, ToDoUser user, DateTime createdAt)
        {
            Id = id;
            Name = name;
            User = user;
            CreatedAt = createdAt;
        }
    }
}
