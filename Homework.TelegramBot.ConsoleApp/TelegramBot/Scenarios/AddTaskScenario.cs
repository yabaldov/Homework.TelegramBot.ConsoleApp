using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Services;
using Homework.TelegramBot.ConsoleApp.TelegramBot.Dto;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

public class AddTaskScenario : IScenario
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoListService _toDoListService;

    public AddTaskScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService)
    {
        _userService = userService;
        _toDoService = toDoService;
        _toDoListService = toDoListService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.AddTask;
    }

    public async Task<ScenarioResult> HandleMessageAsync(
        ITelegramBotClient bot,
        ScenarioContext context,
        Update update,
        CancellationToken ct)
    {
        var chatId = GetChatId(update);

        switch (context.CurrentStep)
        {
            case null:
                return await HandleStartStepAsync(bot, chatId, context, ct);

            case "Name":
                var text = update.Message?.Text?.Trim() ?? string.Empty;
                return await HandleNameStepAsync(bot, chatId, context, text, ct);

            case "Deadline":
                var deadlineText = update.Message?.Text?.Trim() ?? string.Empty;
                return await HandleDeadlineStepAsync(bot, chatId, context, deadlineText, ct);

            case "List":
                return await HandleListStepAsync(bot, chatId, context, update, ct);

            default:
                return ScenarioResult.Completed;
        }
    }

    private async Task<ScenarioResult> HandleStartStepAsync(
        ITelegramBotClient bot,
        long chatId,
        ScenarioContext context,
        CancellationToken ct)
    {
        var user = await _userService.GetUserAsync(context.UserId, ct);
        if (user == null)
        {
            await bot.SendMessage(chatId, "Пользователь не найден. Используйте /start для регистрации.", cancellationToken: ct);
            return ScenarioResult.Completed;
        }

        context.Data["User"] = user;
        context.CurrentStep = "Name";

        await bot.SendMessage(
            chatId,
            "Введите название задачи:",
            replyMarkup: GetCancelKeyboard(),
            cancellationToken: ct);

        return ScenarioResult.Transition;
    }

    private async Task<ScenarioResult> HandleNameStepAsync(
        ITelegramBotClient bot,
        long chatId,
        ScenarioContext context,
        string name,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            await bot.SendMessage(
                chatId,
                "Название задачи не может быть пустым. Введите название задачи:",
                replyMarkup: GetCancelKeyboard(),
                cancellationToken: ct);
            return ScenarioResult.Transition;
        }

        context.Data["Name"] = name;
        context.CurrentStep = "Deadline";

        await bot.SendMessage(
            chatId,
            "Введите срок выполнения (дд.мм.гггг):",
            replyMarkup: GetCancelKeyboard(),
            cancellationToken: ct);

        return ScenarioResult.Transition;
    }

    private async Task<ScenarioResult> HandleDeadlineStepAsync(
        ITelegramBotClient bot,
        long chatId,
        ScenarioContext context,
        string deadlineText,
        CancellationToken ct)
    {
        if (!DateTime.TryParseExact(deadlineText, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var deadline))
        {
            await bot.SendMessage(
                chatId,
                "Неверный формат даты. Введите срок выполнения в формате дд.мм.гггг:",
                replyMarkup: GetCancelKeyboard(),
                cancellationToken: ct);
            return ScenarioResult.Transition;
        }

        if (deadline.Date < DateTime.Today)
        {
            await bot.SendMessage(
                chatId,
                "Срок выполнения не может быть в прошлом. Введите дату сегодня или в будущем:",
                replyMarkup: GetCancelKeyboard(),
                cancellationToken: ct);
            return ScenarioResult.Transition;
        }

        context.Data["Deadline"] = deadline;

        var user = (ToDoUser)context.Data["User"];
        var lists = await _toDoListService.GetUserListsAsync(user.UserId, ct);

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new() { InlineKeyboardButton.WithCallbackData("📌Без списка", new ToDoListCallbackDto { Action = "selectlist", ToDoListId = null }.ToString()) }
        };

        buttons.AddRange(lists.Select(list => new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData(list.Name, new ToDoListCallbackDto { Action = "selectlist", ToDoListId = list.Id }.ToString())
        }));

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);

        await bot.SendMessage(
            chatId,
            "Выберите список для задачи:",
            replyMarkup: inlineKeyboard,
            cancellationToken: ct);

        context.CurrentStep = "List";
        return ScenarioResult.Transition;
    }

    private async Task<ScenarioResult> HandleListStepAsync(
        ITelegramBotClient bot,
        long chatId,
        ScenarioContext context,
        Update update,
        CancellationToken ct)
    {
        var callbackData = update.CallbackQuery?.Data;
        if (string.IsNullOrEmpty(callbackData))
            return ScenarioResult.Transition;

        var listCallback = ToDoListCallbackDto.FromString(callbackData);

        ToDoList? list = null;
        if (listCallback.ToDoListId.HasValue)
        {
            list = await _toDoListService.GetAsync(listCallback.ToDoListId.Value, ct);
        }

        var user = (ToDoUser)context.Data["User"];
        var name = (string)context.Data["Name"];
        var deadline = (DateTime)context.Data["Deadline"];

        var task = await _toDoService.AddAsync(user, name, deadline, list, ct);

        var listInfo = list != null ? $" (список: {list.Name})" : "";
        await bot.SendMessage(
            chatId,
            $"Задача \"{task.Name}\" добавлена{listInfo}. Срок: {task.Deadline:dd.MM.yyyy}",
            replyMarkup: GetMainKeyboard(),
            cancellationToken: ct);

        return ScenarioResult.Completed;
    }

    private static long GetChatId(Update update)
    {
        if (update.Message != null)
            return update.Message.Chat.Id;
        if (update.CallbackQuery?.Message != null)
            return update.CallbackQuery.Message.Chat.Id;
        return 0;
    }

    private static ReplyKeyboardMarkup GetCancelKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "/cancel" }
        })
        {
            ResizeKeyboard = true
        };
    }

    private static ReplyKeyboardMarkup GetMainKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "/addtask", "/show", "/report" }
        })
        {
            ResizeKeyboard = true
        };
    }
}
