using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Services;
using Homework.TelegramBot.ConsoleApp.TelegramBot.Dto;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

public class DeleteListScenario : IScenario
{
    private readonly IUserService _userService;
    private readonly IToDoListService _toDoListService;
    private readonly IToDoService _toDoService;

    public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
    {
        _userService = userService;
        _toDoListService = toDoListService;
        _toDoService = toDoService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.DeleteList;
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

            case "Approve":
                return await HandleApproveStepAsync(bot, chatId, context, update, ct);

            case "Delete":
                return await HandleDeleteStepAsync(bot, chatId, context, update, ct);

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

        var lists = await _toDoListService.GetUserListsAsync(user.UserId, ct);

        if (lists.Count == 0)
        {
            await bot.SendMessage(
                chatId,
                "У вас нет списков для удаления.",
                replyMarkup: GetMainKeyboard(),
                cancellationToken: ct);
            return ScenarioResult.Completed;
        }

        var buttons = new List<List<InlineKeyboardButton>>();

        foreach (var list in lists)
        {
            buttons.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(
                    list.Name,
                    new ToDoListCallbackDto { Action = "deletelist", ToDoListId = list.Id }.ToString())
            });
        }

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);

        await bot.SendMessage(
            chatId,
            "Выберите список для удаления:",
            replyMarkup: inlineKeyboard,
            cancellationToken: ct);

        context.CurrentStep = "Approve";
        return ScenarioResult.Transition;
    }

    private async Task<ScenarioResult> HandleApproveStepAsync(
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
        if (!listCallback.ToDoListId.HasValue)
            return ScenarioResult.Transition;

        var toDoList = await _toDoListService.GetAsync(listCallback.ToDoListId.Value, ct);
        if (toDoList == null)
        {
            await bot.SendMessage(
                chatId,
                "Список не найден.",
                replyMarkup: GetMainKeyboard(),
                cancellationToken: ct);
            return ScenarioResult.Completed;
        }

        context.Data["ToDoList"] = toDoList;
        context.CurrentStep = "Delete";

        var confirmButtons = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                InlineKeyboardButton.WithCallbackData("❌Нет", "no")
            }
        });

        await bot.SendMessage(
            chatId,
            $"Подтверждаете удаление списка \"{toDoList.Name}\" и всех его задач?",
            replyMarkup: confirmButtons,
            cancellationToken: ct);

        return ScenarioResult.Transition;
    }

    private async Task<ScenarioResult> HandleDeleteStepAsync(
        ITelegramBotClient bot,
        long chatId,
        ScenarioContext context,
        Update update,
        CancellationToken ct)
    {
        var callbackData = update.CallbackQuery?.Data;
        if (string.IsNullOrEmpty(callbackData))
            return ScenarioResult.Transition;

        if (callbackData == "yes")
        {
            var user = (ToDoUser)context.Data["User"];
            var toDoList = (ToDoList)context.Data["ToDoList"];

            // Удалить все задачи списка
            var tasks = await _toDoService.GetByUserIdAndListAsync(user.UserId, toDoList.Id, ct);
            foreach (var task in tasks)
            {
                await _toDoService.DeleteAsync(task.Id, ct);
            }

            // Удалить сам список
            await _toDoListService.DeleteAsync(toDoList.Id, ct);

            await bot.SendMessage(
                chatId,
                $"Список \"{toDoList.Name}\" и все его задачи удалены.",
                replyMarkup: GetMainKeyboard(),
                cancellationToken: ct);
        }
        else
        {
            await bot.SendMessage(
                chatId,
                "Удаление отменено.",
                replyMarkup: GetMainKeyboard(),
                cancellationToken: ct);
        }

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
