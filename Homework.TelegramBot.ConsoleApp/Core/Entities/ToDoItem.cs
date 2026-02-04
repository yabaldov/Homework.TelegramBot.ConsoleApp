using System;

namespace Homework.TelegramBot.ConsoleApp.Core.Entities
{
    public class ToDoItem
    {
        public Guid Id { get; }
        public ToDoUser User { get; }
        public string Name { get; }
        public DateTime CreatedAt { get; }
        public DateTime Deadline { get; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }

        public ToDoItem(ToDoUser user, string name, DateTime deadline)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            Deadline = deadline;
            State = ToDoItemState.Active;
            StateChangedAt = null;
        }

        // Конструктор для восстановления из файла
        public ToDoItem(Guid id, ToDoUser user, string name, DateTime createdAt, DateTime deadline, ToDoItemState state, DateTime? stateChangedAt)
        {
            Id = id;
            User = user;
            Name = name;
            CreatedAt = createdAt;
            Deadline = deadline;
            State = state;
            StateChangedAt = stateChangedAt;
        }
    }
}
