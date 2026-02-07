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
using Homework.TelegramBot.ConsoleApp.TelegramBot.Dto;
using Homework.TelegramBot.ConsoleApp.Helpers;
using Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot
{
    public class UpdateHandler : IUpdateHandler
    {
        private static readonly int _pageSize = 5;

        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoReportService _toDoReportService;
        private readonly IToDoListService _toDoListService;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IScenarioContextRepository _contextRepository;

        public UpdateHandler(
            IUserService userService,
            IToDoService toDoService,
            IToDoReportService toDoReportService,
            IToDoListService toDoListService,
            IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoReportService = toDoReportService;
            _toDoListService = toDoListService;
            _scenarios = scenarios;
            _contextRepository = contextRepository;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await (update switch
            {
                { Message: { } message } => OnMessage(botClient, update, message, ct),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(botClient, update, callbackQuery, ct),
                _ => Task.CompletedTask
            });
        }

        private async Task OnMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct)
        {
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
                    case "/show":
                        await HandleShowAsync(botClient, chat, user, ct);
                        break;
                    case "/report":
                        await HandleReportAsync(botClient, chat, user, ct);
                        break;
                    case "/addtask":
                        await HandleAddTaskAsync(botClient, chat, user, update, ct);
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

        private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery query, CancellationToken ct)
        {
            if (query.From == null || query.Message == null || string.IsNullOrEmpty(query.Data))
                return;

            var chat = query.Message.Chat;

            try
            {
                var user = await _userService.GetUserAsync(query.From.Id, ct);
                if (user == null)
                    return;

                // Проверяем, есть ли активный сценарий у пользователя
                var scenarioContext = await _contextRepository.GetContext(query.From.Id, ct);
                if (scenarioContext != null)
                {
                    await ProcessScenarioAsync(botClient, scenarioContext, update, ct);
                    return;
                }

                var callback = CallbackDto.FromString(query.Data);

                switch (callback.Action)
                {
                    case "show":
                        var listCallback = PagedListCallbackDto.FromString(query.Data);
                        await HandleShowTasksByListAsync(botClient, chat, query.Message.MessageId, user, listCallback, ct);
                        break;
                    case "show_completed":
                        var completedCallback = PagedListCallbackDto.FromString(query.Data);
                        await HandleShowCompletedTasksAsync(botClient, chat, query.Message.MessageId, user, completedCallback, ct);
                        break;
                    case "addlist":
                        await StartScenarioAsync(botClient, user, update, ScenarioType.AddList, ct);
                        break;
                    case "deletelist":
                        await StartScenarioAsync(botClient, user, update, ScenarioType.DeleteList, ct);
                        break;
                    case "showtask":
                        var itemCallback = ToDoItemCallbackDto.FromString(query.Data);
                        await HandleShowTaskDetailAsync(botClient, chat, itemCallback.ToDoItemId, ct);
                        break;
                    case "completetask":
                        var completeCallback = ToDoItemCallbackDto.FromString(query.Data);
                        await HandleCompleteTaskByCallbackAsync(botClient, chat, completeCallback.ToDoItemId, ct);
                        break;
                    case "deletetask":
                        var deleteCallback = ToDoItemCallbackDto.FromString(query.Data);
                        await HandleDeleteTaskByCallbackAsync(botClient, chat, deleteCallback.ToDoItemId, ct);
                        break;
                }

                await botClient.AnswerCallbackQuery(query.Id, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chat.Id, $"Ошибка: {ex.Message}", cancellationToken: ct);
            }
        }

        private async Task HandleShowTasksByListAsync(ITelegramBotClient botClient, Chat chat, int messageId, ToDoUser user, PagedListCallbackDto listDto, CancellationToken ct)
        {
            var allTasks = await _toDoService.GetByUserIdAndListAsync(user.UserId, listDto.ToDoListId, ct);
            var activeTasks = allTasks.Where(t => t.State == ToDoItemState.Active).ToList();

            if (activeTasks.Count == 0)
            {
                await botClient.EditMessageText(chat.Id, messageId, "Задач нет", cancellationToken: ct);
                return;
            }

            var callbackData = activeTasks
                .Select(t => new KeyValuePair<string, string>(
                    t.Name,
                    new ToDoItemCallbackDto { Action = "showtask", ToDoItemId = t.Id }.ToString()))
                .ToList();

            var inlineKeyboard = BuildPagedButtons(callbackData, listDto);
            AppendCompletedButton(inlineKeyboard, listDto.ToDoListId);
            await botClient.EditMessageText(chat.Id, messageId, "Задачи:", replyMarkup: inlineKeyboard, cancellationToken: ct);
        }

        private async Task HandleShowCompletedTasksAsync(ITelegramBotClient botClient, Chat chat, int messageId, ToDoUser user, PagedListCallbackDto listDto, CancellationToken ct)
        {
            var allTasks = await _toDoService.GetByUserIdAndListAsync(user.UserId, listDto.ToDoListId, ct);
            var completedTasks = allTasks.Where(t => t.State == ToDoItemState.Completed).ToList();

            if (completedTasks.Count == 0)
            {
                await botClient.EditMessageText(chat.Id, messageId, "Задач нет", cancellationToken: ct);
                return;
            }

            var callbackData = completedTasks
                .Select(t => new KeyValuePair<string, string>(
                    $"☑️ {t.Name}",
                    new ToDoItemCallbackDto { Action = "showtask", ToDoItemId = t.Id }.ToString()))
                .ToList();

            var inlineKeyboard = BuildPagedButtons(callbackData, listDto);
            await botClient.EditMessageText(chat.Id, messageId, "Выполненные задачи:", replyMarkup: inlineKeyboard, cancellationToken: ct);
        }

        private static void AppendCompletedButton(InlineKeyboardMarkup markup, Guid? listId)
        {
            var rows = markup.InlineKeyboard.ToList();
            rows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("☑️Посмотреть выполненные",
                    new PagedListCallbackDto { Action = "show_completed", ToDoListId = listId, Page = 0 }.ToString())
            });
            markup.InlineKeyboard = rows;
        }

        private async Task StartScenarioAsync(ITelegramBotClient botClient, ToDoUser user, Update update, ScenarioType scenarioType, CancellationToken ct)
        {
            var context = new ScenarioContext(scenarioType)
            {
                UserId = user.TelegramUserId
            };

            await ProcessScenarioAsync(botClient, context, update, ct);
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
            await SendMessageAsync(botClient, chat, $"Привет, {greeting}!\nТеперь вам доступны команды: /addtask, /show, /report, /find, /exit", true, ct);
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
                        "/show -- показать списки и задачи.\n" +
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

        private async Task HandleShowAsync(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
        {
            if (user == null)
            {
                await SendMessageAsync(botClient, chat, "Сначала используйте команду /start для регистрации.", false, ct);
                return;
            }

            var lists = await _toDoListService.GetUserListsAsync(user.UserId, ct);

            var buttons = new List<List<InlineKeyboardButton>>
            {
                new() { InlineKeyboardButton.WithCallbackData("📌Без списка", new PagedListCallbackDto { Action = "show", ToDoListId = null, Page = 0 }.ToString()) }
            };

            foreach (var list in lists)
            {
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(list.Name, new PagedListCallbackDto { Action = "show", ToDoListId = list.Id, Page = 0 }.ToString())
                });
            }

            buttons.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist"),
                InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist")
            });

            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

            await botClient.SendMessage(
                chat.Id,
                "Выберите список",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);
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

        private async Task HandleShowTaskDetailAsync(ITelegramBotClient botClient, Chat chat, Guid toDoItemId, CancellationToken ct)
        {
            var task = await _toDoService.GetAsync(toDoItemId, ct);
            if (task == null)
            {
                await botClient.SendMessage(chat.Id, "Задача не найдена.", cancellationToken: ct);
                return;
            }

            var message = $"Задача: {task.Name}\n" +
                          $"Статус: {task.State}\n" +
                          $"Создана: {task.CreatedAt:dd.MM.yyyy HH:mm:ss}\n" +
                          $"Срок: {task.Deadline:dd.MM.yyyy}";

            var buttons = new List<List<InlineKeyboardButton>>
            {
                new()
                {
                    InlineKeyboardButton.WithCallbackData("✅Выполнить",
                        new ToDoItemCallbackDto { Action = "completetask", ToDoItemId = task.Id }.ToString()),
                    InlineKeyboardButton.WithCallbackData("❌Удалить",
                        new ToDoItemCallbackDto { Action = "deletetask", ToDoItemId = task.Id }.ToString())
                }
            };

            await botClient.SendMessage(chat.Id, message, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: ct);
        }

        private async Task HandleCompleteTaskByCallbackAsync(ITelegramBotClient botClient, Chat chat, Guid toDoItemId, CancellationToken ct)
        {
            var task = await _toDoService.GetAsync(toDoItemId, ct);
            if (task == null)
            {
                await botClient.SendMessage(chat.Id, "Задача не найдена.", cancellationToken: ct);
                return;
            }

            await _toDoService.MarkCompletedAsync(toDoItemId, ct);
            await botClient.SendMessage(chat.Id, $"Задача \"{task.Name}\" завершена.", cancellationToken: ct);
        }

        private async Task HandleDeleteTaskByCallbackAsync(ITelegramBotClient botClient, Chat chat, Guid toDoItemId, CancellationToken ct)
        {
            var task = await _toDoService.GetAsync(toDoItemId, ct);
            if (task == null)
            {
                await botClient.SendMessage(chat.Id, "Задача не найдена.", cancellationToken: ct);
                return;
            }

            await _toDoService.DeleteAsync(toDoItemId, ct);
            await botClient.SendMessage(chat.Id, $"Задача \"{task.Name}\" удалена.", cancellationToken: ct);
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

        private static InlineKeyboardMarkup BuildPagedButtons(
            IReadOnlyList<KeyValuePair<string, string>> callbackData,
            PagedListCallbackDto listDto)
        {
            var totalPages = (int)Math.Ceiling((double)callbackData.Count / _pageSize);
            var pageItems = callbackData.GetBatchByNumber(_pageSize, listDto.Page);

            var buttons = new List<List<InlineKeyboardButton>>();
            foreach (var item in pageItems)
            {
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(item.Key, item.Value)
                });
            }

            var navigationRow = new List<InlineKeyboardButton>();
            if (listDto.Page > 0)
            {
                navigationRow.Add(InlineKeyboardButton.WithCallbackData("⬅️",
                    new PagedListCallbackDto { Action = listDto.Action, ToDoListId = listDto.ToDoListId, Page = listDto.Page - 1 }.ToString()));
            }
            if (listDto.Page < totalPages - 1)
            {
                navigationRow.Add(InlineKeyboardButton.WithCallbackData("➡️",
                    new PagedListCallbackDto { Action = listDto.Action, ToDoListId = listDto.ToDoListId, Page = listDto.Page + 1 }.ToString()));
            }
            if (navigationRow.Count > 0)
            {
                buttons.Add(navigationRow);
            }

            return new InlineKeyboardMarkup(buttons);
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
                new KeyboardButton[] { "/addtask", "/show", "/report" }
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
