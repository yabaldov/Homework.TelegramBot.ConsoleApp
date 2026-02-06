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
        public ToDoList? List { get; }

        public ToDoItem(ToDoUser user, string name, DateTime deadline, ToDoList? list = null)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            Deadline = deadline;
            State = ToDoItemState.Active;
            StateChangedAt = null;
            List = list;
        }

        // Конструктор для восстановления из файла
        public ToDoItem(Guid id, ToDoUser user, string name, DateTime createdAt, DateTime deadline, ToDoItemState state, DateTime? stateChangedAt, ToDoList? list = null)
        {
            Id = id;
            User = user;
            Name = name;
            CreatedAt = createdAt;
            Deadline = deadline;
            State = state;
            StateChangedAt = stateChangedAt;
            List = list;
        }
    }
}
