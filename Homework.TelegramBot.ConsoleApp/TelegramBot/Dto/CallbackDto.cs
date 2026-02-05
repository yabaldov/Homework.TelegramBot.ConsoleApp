using System;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Dto
{
    public class CallbackDto
    {
        public string Action { get; set; } = string.Empty;

        public static CallbackDto FromString(string input)
        {
            var parts = input.Split('|');
            return new CallbackDto
            {
                Action = parts[0]
            };
        }

        public override string ToString()
        {
            return Action;
        }
    }
}
