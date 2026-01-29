using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Homework.TelegramBot.ConsoleApp.Core.Services;
using Homework.TelegramBot.ConsoleApp.Core.Validation;
using Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess;
using Homework.TelegramBot.ConsoleApp.TelegramBot;

namespace Homework.TelegramBot.ConsoleApp
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
                ?? throw new InvalidOperationException("Не задана переменная окружения TELEGRAM_BOT_TOKEN");

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

            Console.WriteLine($"Добро пожаловать в Telegram ToDo Bot!{Environment.NewLine}");

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

            var botClient = new TelegramBotClient(token);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message },
                DropPendingUpdates = true
            };

            await botClient.SetMyCommands(new[]
            {
                new BotCommand { Command = "start", Description = "Регистрация" },
                new BotCommand { Command = "help", Description = "Список команд" },
                new BotCommand { Command = "info", Description = "Информация о боте" },
                new BotCommand { Command = "addtask", Description = "Добавить задачу" },
                new BotCommand { Command = "showtasks", Description = "Показать активные задачи" },
                new BotCommand { Command = "showalltasks", Description = "Показать все задачи" },
                new BotCommand { Command = "removetask", Description = "Удалить задачу" },
                new BotCommand { Command = "completetask", Description = "Завершить задачу" },
                new BotCommand { Command = "find", Description = "Найти задачу по началу названия" },
                new BotCommand { Command = "report", Description = "Статистика" }
            }, cancellationToken: cts.Token);

            botClient.StartReceiving(updateHandler, receiverOptions, cts.Token);

            var me = await botClient.GetMe(cts.Token);
            Console.WriteLine($"{me.FirstName} (@{me.Username}) запущен!");
            Console.WriteLine();
            Console.WriteLine("Нажмите клавишу \"A\" для выхода");

            while (!cts.Token.IsCancellationRequested)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.A)
                {
                    cts.Cancel();
                    break;
                }
                else
                {
                    var botInfo = await botClient.GetMe(cts.Token);
                    Console.WriteLine($"Бот: {botInfo.FirstName} (@{botInfo.Username}), Id: {botInfo.Id}");
                }
            }

            Console.WriteLine("Программа завершена.");
        }
    }
}
