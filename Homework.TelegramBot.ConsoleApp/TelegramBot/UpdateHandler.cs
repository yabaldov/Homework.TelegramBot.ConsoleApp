using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Services;
using Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoReportService _toDoReportService;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IScenarioContextRepository _contextRepository;

        public UpdateHandler(
            IUserService userService,
            IToDoService toDoService,
            IToDoReportService toDoReportService,
            IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoReportService = toDoReportService;
            _scenarios = scenarios;
            _contextRepository = contextRepository;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is not { } message)
                return;

            var chat = message.Chat;
            var from = message.From;
            var text = message.Text?.Trim() ?? string.Empty;

            if (from == null)
                return;

            try
            {
                var user = await _userService.GetUserAsync(from.Id, ct);

                // Обработка команды /cancel до проверки активного сценария
                if (text == "/cancel")
                {
                    await HandleCancelAsync(botClient, chat, from.Id, user, ct);
                    return;
                }

                // Проверяем, есть ли активный сценарий у пользователя
                var scenarioContext = await _contextRepository.GetContext(from.Id, ct);
                if (scenarioContext != null)
                {
                    await ProcessScenarioAsync(botClient, scenarioContext, update, ct);
                    return;
                }

                switch (text)
                {
                    case "/start":
                        await HandleStartAsync(botClient, chat, from, user, ct);
                        break;
                    case "/help":
                        await HandleHelpAsync(botClient, chat, user, ct);
                        break;
                    case "/info":
                        await HandleInfoAsync(botClient, chat, user, ct);
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
                    case "/addtask":
                        await HandleAddTaskAsync(botClient, chat, user, update, ct);
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
                        await HandleExitAsync(botClient, chat, user, ct);
                        break;
                    default:
                        await SendMessageAsync(botClient, chat, "Неизвестная команда. Введите /help для списка доступных команд.", user != null, ct);
                        break;
                }
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chat.Id, $"Ошибка: {ex.Message}", cancellationToken: ct);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка обработки ({source}): {exception.GetType().Name}: {exception.Message}");
            return Task.CompletedTask;
        }

        private IScenario GetScenario(ScenarioType scenarioType)
        {
            var scenario = _scenarios.FirstOrDefault(s => s.CanHandle(scenarioType));
            if (scenario == null)
            {
                throw new InvalidOperationException($"Сценарий для типа {scenarioType} не найден.");
            }
            return scenario;
        }

        private async Task ProcessScenarioAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);
            var result = await scenario.HandleMessageAsync(botClient, context, update, ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(context.UserId, ct);
            }
            else
            {
                await _contextRepository.SetContext(context.UserId, context, ct);
            }
        }

        private async Task HandleStartAsync(ITelegramBotClient botClient, Chat chat, User from, ToDoUser? user, CancellationToken ct)
        {
            if (user != null)
            {
                var firstName = from.FirstName ?? user.TelegramUserName;
                await SendMessageAsync(botClient, chat, $"Вы уже зарегистрированы как {firstName}.", true, ct);
                return;
            }

            var userName = from.Username ?? $"User_{from.Id}";
            var newUser = await _userService.RegisterUserAsync(from.Id, userName, ct);
            var greeting = from.FirstName ?? newUser.TelegramUserName;
            await SendMessageAsync(botClient, chat, $"Привет, {greeting}!\nТеперь вам доступны команды: /addtask, /showtasks, /showalltasks, /removetask, /completetask, /report, /find, /exit", true, ct);
        }

        private async Task HandleHelpAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            var help = "Список доступных команд:\n" +
                       "/start -- начать работу (регистрация).\n" +
                       "/help -- показать эту справку.\n" +
                       "/info -- показать информацию о программе.\n";

            if (user != null)
            {
                help += "/addtask -- добавить задачу.\n" +
                        "/showtasks -- показать активные задачи.\n" +
                        "/showalltasks -- показать все задачи.\n" +
                        "/completetask <Id> -- завершить задачу по Id.\n" +
                        "/removetask <номер> -- удалить задачу по номеру.\n" +
                        "/report -- статистика по задачам.\n" +
                        "/find <префикс> -- найти задачи по началу названия.\n" +
                        "/cancel -- отменить текущий сценарий.\n" +
                        "/exit -- выйти из программы.";
            }

            await SendMessageAsync(botClient, chat, help, user != null, ct);
        }

        private async Task HandleInfoAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            await SendMessageAsync(botClient, chat, "Программа: Telegram ToDo Bot.\nВерсия: 1.0.0\nДата создания: 2026-01-29", user != null, ct);
        }

        private async Task HandleShowTasksAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            if (user == null)
            {
                await SendMessageAsync(botClient, chat, "Сначала используйте команду /start для регистрации.", false, ct);
                return;
            }

            var tasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, ct);

            if (tasks.Count == 0)
            {
                await SendMessageAsync(botClient, chat, "Список задач пуст.", true, ct);
                return;
            }

            var message = "Ваши задачи:\n";
            foreach (var task in tasks)
            {
                message += $"{task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - `{task.Id}`\n";
            }

            await SendMessageAsync(botClient, chat, message.TrimEnd(), true, ct);
        }

        private async Task HandleShowAllTasksAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            if (user == null)
            {
                await SendMessageAsync(botClient, chat, "Сначала используйте команду /start для регистрации.", false, ct);
                return;
            }

            var tasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);

            if (tasks.Count == 0)
            {
                await SendMessageAsync(botClient, chat, "Список задач пуст.", true, ct);
                return;
            }

            var message = "Все задачи:\n";
            foreach (var task in tasks)
            {
                message += $"({task.State}) {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - `{task.Id}`\n";
            }

            await SendMessageAsync(botClient, chat, message.TrimEnd(), true, ct);
        }

        private async Task HandleReportAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            if (user == null)
            {
                await SendMessageAsync(botClient, chat, "Сначала используйте команду /start для регистрации.", false, ct);
                return;
            }

            var (total, completed, active, generatedAt) = await _toDoReportService.GetUserStatsAsync(user.UserId, ct);

            var message = $"Статистика по задачам на {generatedAt:dd.MM.yyyy HH:mm:ss}. " +
                          $"Всего: {total}; Завершенных: {completed}; Активных: {active};";

            await SendMessageAsync(botClient, chat, message, true, ct);
        }

        private async Task HandleFindAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command, CancellationToken ct)
        {
            if (user == null)
            {
                await SendMessageAsync(botClient, chat, "Сначала используйте команду /start для регистрации.", false, ct);
                return;
            }

            var namePrefix = command.Length > 5 ? command.Substring(5).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(namePrefix))
            {
                await SendMessageAsync(botClient, chat, "Пожалуйста, укажите начало названия задачи. Пример: /find Купить", true, ct);
                return;
            }

            var tasks = await _toDoService.FindAsync(user, namePrefix, ct);

            if (tasks.Count == 0)
            {
                await SendMessageAsync(botClient, chat, $"Задачи, начинающиеся с \"{namePrefix}\", не найдены.", true, ct);
                return;
            }

            var message = "Найденные задачи:\n";
            foreach (var task in tasks)
            {
                message += $"{task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - `{task.Id}`\n";
            }

            await SendMessageAsync(botClient, chat, message.TrimEnd(), true, ct);
        }

        private async Task HandleAddTaskAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, Update update, CancellationToken ct)
        {
            if (user == null)
            {
                await SendMessageAsync(botClient, chat, "Сначала используйте команду /start для регистрации.", false, ct);
                return;
            }

            var context = new ScenarioContext(ScenarioType.AddTask)
            {
                UserId = user.TelegramUserId
            };

            await ProcessScenarioAsync(botClient, context, update, ct);
        }

        private async Task HandleRemoveTaskAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command, CancellationToken ct)
        {
            if (user == null)
            {
                await SendMessageAsync(botClient, chat, "Сначала используйте команду /start для регистрации.", false, ct);
                return;
            }

            var numberPart = command.Length > 11 ? command.Substring(11).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(numberPart))
            {
                var tasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);
                if (tasks.Count == 0)
                {
                    await SendMessageAsync(botClient, chat, "Список задач пуст. Удаление невозможно.", true, ct);
                    return;
                }

                var message = "Укажите номер задачи для удаления. Пример: /removetask 1\n\nВаши задачи:\n";
                for (int i = 0; i < tasks.Count; i++)
                {
                    message += $"{i + 1}. {tasks[i].Name}\n";
                }
                await SendMessageAsync(botClient, chat, message.TrimEnd(), true, ct);
                return;
            }

            if (!int.TryParse(numberPart, out int taskNumber))
            {
                await SendMessageAsync(botClient, chat, "Неверный формат номера задачи. Пример: /removetask 1", true, ct);
                return;
            }

            var allTasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);

            if (taskNumber < 1 || taskNumber > allTasks.Count)
            {
                await SendMessageAsync(botClient, chat, $"Неверный номер задачи. Доступные номера: 1-{allTasks.Count}", true, ct);
                return;
            }

            var taskToRemove = allTasks[taskNumber - 1];
            await _toDoService.DeleteAsync(taskToRemove.Id, ct);
            await SendMessageAsync(botClient, chat, $"Задача \"{taskToRemove.Name}\" удалена.", true, ct);
        }

        private async Task HandleCompleteTaskAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command, CancellationToken ct)
        {
            if (user == null)
            {
                await SendMessageAsync(botClient, chat, "Сначала используйте команду /start для регистрации.", false, ct);
                return;
            }

            var idPart = command.Length > 13 ? command.Substring(13).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(idPart))
            {
                await SendMessageAsync(botClient, chat, "Пожалуйста, укажите Id задачи. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e", true, ct);
                return;
            }

            if (!Guid.TryParse(idPart, out Guid taskId))
            {
                await SendMessageAsync(botClient, chat, "Неверный формат Id. Пожалуйста, введите корректный GUID.", true, ct);
                return;
            }

            var tasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);
            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
            {
                await SendMessageAsync(botClient, chat, "Задача с указанным Id не найдена.", true, ct);
                return;
            }

            await _toDoService.MarkCompletedAsync(taskId, ct);
            await SendMessageAsync(botClient, chat, $"Задача \"{task.Name}\" завершена.", true, ct);
        }

        private async Task HandleExitAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            await SendMessageAsync(botClient, chat, "До свидания! Для продолжения работы используйте /start.", user != null, ct);
        }

        private async Task HandleCancelAsync(ITelegramBotClient botClient, Chat chat, long telegramUserId, ToDoUser? user, CancellationToken ct)
        {
            var context = await _contextRepository.GetContext(telegramUserId, ct);

            if (context == null)
            {
                await SendMessageAsync(botClient, chat, "Нет активного сценария для отмены.", user != null, ct);
                return;
            }

            await _contextRepository.ResetContext(telegramUserId, ct);
            await SendMessageAsync(botClient, chat, "Сценарий отменён.", user != null, ct);
        }

        private static ReplyKeyboardMarkup GetKeyboard(bool isRegistered)
        {
            if (!isRegistered)
            {
                return new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "/start" }
                })
                {
                    ResizeKeyboard = true
                };
            }

            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "/addtask", "/showtasks", "/showalltasks", "/report" }
            })
            {
                ResizeKeyboard = true
            };
        }

        private static async Task SendMessageAsync(ITelegramBotClient botClient, Chat chat, string text, bool isRegistered, CancellationToken ct)
        {
            await botClient.SendMessage(
                chat.Id,
                text,
                parseMode: ParseMode.Markdown,
                replyMarkup: GetKeyboard(isRegistered),
                cancellationToken: ct);
        }
    }
}
