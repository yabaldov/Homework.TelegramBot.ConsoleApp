using System;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess.Dto
{
    public class ToDoItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }

        public static ToDoItemDto FromEntity(ToDoItem item)
        {
            return new ToDoItemDto
            {
                Id = item.Id,
                UserId = item.User.UserId,
                Name = item.Name,
                CreatedAt = item.CreatedAt,
                State = item.State,
                StateChangedAt = item.StateChangedAt
            };
        }

        public ToDoItem ToEntity(ToDoUser user)
        {
            return new ToDoItem(Id, user, Name, CreatedAt, State, StateChangedAt);
        }
    }
}
