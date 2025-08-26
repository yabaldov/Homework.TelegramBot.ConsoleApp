using System;

namespace Homework.TelegramBot.ConsoleApp
{

    public class StringValidator
    {
        public static int ParseAndValidateInt(string? str, int min, int max)
        {
            if (int.TryParse(str, out int value))
            {
                if (value >= min && value <= max)
                {
                    return value;
                }
            }
            throw new ArgumentException($"Строка не является целым числом в диапазоне от {min} до {max}. Исходная строка: \"{str}\"");
        }

        public static void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException($"Строка не может быть пустой или состоять только из пробелов. Исходная строка: \"{str}\"");
            }
        }

    }

}