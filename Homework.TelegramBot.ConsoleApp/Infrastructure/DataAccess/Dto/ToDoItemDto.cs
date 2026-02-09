using System;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess.Dto
{
    public class ToDoItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ListId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime Deadline { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }

        public static ToDoItemDto FromEntity(ToDoItem item)
        {
            return new ToDoItemDto
            {
                Id = item.Id,
                UserId = item.User.UserId,
                ListId = item.List?.Id,
                Name = item.Name,
                CreatedAt = item.CreatedAt,
                Deadline = item.Deadline,
                State = item.State,
                StateChangedAt = item.StateChangedAt
            };
        }

        public ToDoItem ToEntity(ToDoUser user, ToDoList? list = null)
        {
            return new ToDoItem
            {
                Id = Id,
                User = user,
                Name = Name,
                CreatedAt = CreatedAt,
                Deadline = Deadline,
                State = State,
                StateChangedAt = StateChangedAt,
                List = list
            };
        }
    }
}
