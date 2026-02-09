using System;

namespace Homework.TelegramBot.ConsoleApp.Core.Entities
{
    public class ToDoList
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ToDoUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
