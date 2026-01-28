using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Services;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoReportService _toDoReportService;

        public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoReportService = toDoReportService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var chat = update.Message.Chat;
            var from = update.Message.From;
            var text = update.Message.Text?.Trim() ?? string.Empty;

            try
            {
                var user = _userService.GetUser(from.Id);

                switch (text)
                {
                    case "/start":
                        await HandleStartAsync(botClient, chat, from, user, ct);
                        break;
                    case "/help":
                        await HandleHelpAsync(botClient, chat, user, ct);
                        break;
                    case "/info":
                        await HandleInfoAsync(botClient, chat, ct);
                        break;
                    case "/showtasks":
                        await HandleShowTasksAsync(botClient, chat, user, ct);
                        break;
                    case "/showalltasks":
                        await HandleShowAllTasksAsync(botClient, chat, user, ct);
                        break;
                    case "/report":
                        await HandleReportAsync(botClient, chat, user, ct);
                        break;
                    case string cmd when cmd.StartsWith("/addtask"):
                        await HandleAddTaskAsync(botClient, chat, user, cmd, ct);
                        break;
                    case string cmd when cmd.StartsWith("/removetask"):
                        await HandleRemoveTaskAsync(botClient, chat, user, cmd, ct);
                        break;
                    case string cmd when cmd.StartsWith("/completetask"):
                        await HandleCompleteTaskAsync(botClient, chat, user, cmd, ct);
                        break;
                    case string cmd when cmd.StartsWith("/find"):
                        await HandleFindAsync(botClient, chat, user, cmd, ct);
                        break;
                    case "/exit":
                        await HandleExitAsync(botClient, chat, ct);
                        break;
                    default:
                        await botClient.SendMessage(chat, "Неизвестная команда. Введите /help для списка доступных команд.", ct);
                        break;
                }
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chat, $"Ошибка: {ex.Message}", ct);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка обработки: {exception.GetType().Name}: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task HandleStartAsync(ITelegramBotClient botClient, Chat chat, User from, ToDoUser? user, CancellationToken ct)
        {
            if (user != null)
            {
                await botClient.SendMessage(chat, $"Вы уже зарегистрированы как {user.TelegramUserName}.", ct);
                return;
            }

            var userName = from.Username ?? $"User_{from.Id}";
            var newUser = _userService.RegisterUser(from.Id, userName);
            await botClient.SendMessage(chat, $"Привет, {newUser.TelegramUserName}!", ct);
            await botClient.SendMessage(chat, "Теперь вам доступны команды: /addtask, /showtasks, /showalltasks, /removetask, /completetask, /report, /find, /exit", ct);
        }

        private async Task HandleHelpAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            var help = "Список доступных команд:\n" +
                       "/start -- начать работу (регистрация).\n" +
                       "/help -- показать эту справку.\n" +
                       "/info -- показать информацию о программе.\n";

            if (user != null)
            {
                help += "/addtask <название> -- добавить задачу.\n" +
                        "/showtasks -- показать активные задачи.\n" +
                        "/showalltasks -- показать все задачи.\n" +
                        "/completetask <Id> -- завершить задачу по Id.\n" +
                        "/removetask <номер> -- удалить задачу по номеру.\n" +
                        "/report -- статистика по задачам.\n" +
                        "/find <префикс> -- найти задачи по началу названия.\n" +
                        "/exit -- выйти из программы.";
            }

            await botClient.SendMessage(chat, help, ct);
        }

        private async Task HandleInfoAsync(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat, "Программа: Симулятор бота Телеграм.\nВерсия: 0.0.4\nДата создания: 2025-08-22", ct);
        }

        private async Task HandleShowTasksAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.", ct);
                return;
            }

            var tasks = _toDoService.GetActiveByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                await botClient.SendMessage(chat, "Список задач пуст.", ct);
                return;
            }

            var message = "Ваши задачи:\n";
            foreach (var task in tasks)
            {
                message += $"{task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
            }

            await botClient.SendMessage(chat, message.TrimEnd(), ct);
        }

        private async Task HandleShowAllTasksAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.", ct);
                return;
            }

            var tasks = _toDoService.GetAllByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                await botClient.SendMessage(chat, "Список задач пуст.", ct);
                return;
            }

            var message = "Все задачи:\n";
            foreach (var task in tasks)
            {
                message += $"({task.State}) {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
            }

            await botClient.SendMessage(chat, message.TrimEnd(), ct);
        }

        private async Task HandleReportAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.", ct);
                return;
            }

            var (total, completed, active, generatedAt) = _toDoReportService.GetUserStats(user.UserId);

            var message = $"Статистика по задачам на {generatedAt:dd.MM.yyyy HH:mm:ss}. " +
                          $"Всего: {total}; Завершенных: {completed}; Активных: {active};";

            await botClient.SendMessage(chat, message, ct);
        }

        private async Task HandleFindAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command, CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.", ct);
                return;
            }

            var namePrefix = command.Length > 5 ? command.Substring(5).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(namePrefix))
            {
                await botClient.SendMessage(chat, "Пожалуйста, укажите начало названия задачи. Пример: /find Купить", ct);
                return;
            }

            var tasks = _toDoService.Find(user, namePrefix);

            if (tasks.Count == 0)
            {
                await botClient.SendMessage(chat, $"Задачи, начинающиеся с \"{namePrefix}\", не найдены.", ct);
                return;
            }

            var message = "Найденные задачи:\n";
            foreach (var task in tasks)
            {
                message += $"{task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
            }

            await botClient.SendMessage(chat, message.TrimEnd(), ct);
        }

        private async Task HandleAddTaskAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command, CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.", ct);
                return;
            }

            var taskName = command.Length > 8 ? command.Substring(8).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(taskName))
            {
                await botClient.SendMessage(chat, "Пожалуйста, укажите название задачи. Пример: /addtask Купить молоко", ct);
                return;
            }

            var task = _toDoService.Add(user, taskName);
            await botClient.SendMessage(chat, $"Задача \"{task.Name}\" добавлена.", ct);
        }

        private async Task HandleRemoveTaskAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command, CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.", ct);
                return;
            }

            var numberPart = command.Length > 11 ? command.Substring(11).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(numberPart))
            {
                var tasks = _toDoService.GetAllByUserId(user.UserId);
                if (tasks.Count == 0)
                {
                    await botClient.SendMessage(chat, "Список задач пуст. Удаление невозможно.", ct);
                    return;
                }

                var message = "Укажите номер задачи для удаления. Пример: /removetask 1\n\nВаши задачи:\n";
                for (int i = 0; i < tasks.Count; i++)
                {
                    message += $"{i + 1}. {tasks[i].Name}\n";
                }
                await botClient.SendMessage(chat, message.TrimEnd(), ct);
                return;
            }

            if (!int.TryParse(numberPart, out int taskNumber))
            {
                await botClient.SendMessage(chat, "Неверный формат номера задачи. Пример: /removetask 1", ct);
                return;
            }

            var allTasks = _toDoService.GetAllByUserId(user.UserId);

            if (taskNumber < 1 || taskNumber > allTasks.Count)
            {
                await botClient.SendMessage(chat, $"Неверный номер задачи. Доступные номера: 1-{allTasks.Count}", ct);
                return;
            }

            var taskToRemove = allTasks[taskNumber - 1];
            _toDoService.Delete(taskToRemove.Id);
            await botClient.SendMessage(chat, $"Задача \"{taskToRemove.Name}\" удалена.", ct);
        }

        private async Task HandleCompleteTaskAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command, CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.", ct);
                return;
            }

            var idPart = command.Length > 13 ? command.Substring(13).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(idPart))
            {
                await botClient.SendMessage(chat, "Пожалуйста, укажите Id задачи. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e", ct);
                return;
            }

            if (!Guid.TryParse(idPart, out Guid taskId))
            {
                await botClient.SendMessage(chat, "Неверный формат Id. Пожалуйста, введите корректный GUID.", ct);
                return;
            }

            var tasks = _toDoService.GetAllByUserId(user.UserId);
            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
            {
                await botClient.SendMessage(chat, "Задача с указанным Id не найдена.", ct);
                return;
            }

            _toDoService.MarkCompleted(taskId);
            await botClient.SendMessage(chat, $"Задача \"{task.Name}\" завершена.", ct);
        }

        private async Task HandleExitAsync(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat, "Программа завершена.", ct);
            Environment.Exit(0);
        }

    }
}
