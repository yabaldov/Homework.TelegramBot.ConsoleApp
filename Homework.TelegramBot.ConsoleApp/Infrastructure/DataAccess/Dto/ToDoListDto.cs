using System;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess.Dto
{
    public class ToDoListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public static ToDoListDto FromEntity(ToDoList list)
        {
            return new ToDoListDto
            {
                Id = list.Id,
                Name = list.Name,
                UserId = list.User.UserId,
                CreatedAt = list.CreatedAt
            };
        }

        public ToDoList ToEntity(ToDoUser user)
        {
            return new ToDoList(Id, Name, user, CreatedAt);
        }
    }
}
