using System;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }

        public static new ToDoListCallbackDto FromString(string input)
        {
            var parts = input.Split('|');
            var dto = new ToDoListCallbackDto
            {
                Action = parts[0]
            };

            if (parts.Length > 1 && Guid.TryParse(parts[1], out var listId))
            {
                dto.ToDoListId = listId;
            }

            return dto;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}
