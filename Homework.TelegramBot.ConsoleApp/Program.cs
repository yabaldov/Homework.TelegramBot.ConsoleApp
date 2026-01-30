using System;
using System.Threading;
using Otus.ToDoList.ConsoleBot;
using Homework.TelegramBot.ConsoleApp.Core.Services;
using Homework.TelegramBot.ConsoleApp.Core.Validation;
using Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess;
using Homework.TelegramBot.ConsoleApp.TelegramBot;

namespace Homework.TelegramBot.ConsoleApp
{
    public static class Program
    {
        static void Main(string[] args)
        {
            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("\nЗавершение работы бота...");
            };
            const int minTasksLimit = 1;
            const int maxTasksLimit = 100;
            const int minTaskLength = 1;
            const int maxTaskLength = 100;

            Console.WriteLine($"Добро пожаловать в симулятор бота Телеграм!{Environment.NewLine}");

            int tasksLimit = 0;
            int taskLengthLimit = 0;

            while (tasksLimit == 0)
            {
                try
                {
                    Console.Write("Введите максимально допустимое количество задач (1-100): ");
                    string? input = Console.ReadLine();
                    tasksLimit = StringValidator.ParseAndValidateInt(input, minTasksLimit, maxTasksLimit);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }

            while (taskLengthLimit == 0)
            {
                try
                {
                    Console.Write("Введите максимально допустимую длину задачи (1-100): ");
                    string? input = Console.ReadLine();
                    taskLengthLimit = StringValidator.ParseAndValidateInt(input, minTaskLength, maxTaskLength);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }

            var userRepository = new InMemoryUserRepository();
            var toDoRepository = new InMemoryToDoRepository();

            var userService = new UserService(userRepository);
            var toDoService = new ToDoService(toDoRepository, tasksLimit, taskLengthLimit);
            var toDoReportService = new ToDoReportService(toDoRepository);

            var updateHandler = new UpdateHandler(userService, toDoService, toDoReportService);

            updateHandler.OnHandleUpdateStarted += OnUpdateStarted;
            updateHandler.OnHandleUpdateCompleted += OnUpdateCompleted;

            try
            {
                ITelegramBotClient botClient = new ConsoleBotClient();
                botClient.StartReceiving(updateHandler, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при работе бота: {ex.Message}");
            }
            finally
            {
                updateHandler.OnHandleUpdateStarted -= OnUpdateStarted;
                updateHandler.OnHandleUpdateCompleted -= OnUpdateCompleted;
            }

            Console.WriteLine("Программа завершена.");
        }

        private static void OnUpdateStarted(string message)
        {
            Console.WriteLine($"Началась обработка сообщения '{message}'");
        }

        private static void OnUpdateCompleted(string message)
        {
            Console.WriteLine($"Закончилась обработка сообщения '{message}'");
        }
    }
}
