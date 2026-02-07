using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Services;
using Homework.TelegramBot.ConsoleApp.TelegramBot.Dto;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

public class DeleteTaskScenario : IScenario
{
    private readonly IToDoService _toDoService;

    public DeleteTaskScenario(IToDoService toDoService)
    {
        _toDoService = toDoService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.DeleteTask;
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
        if (!context.Data.ContainsKey("ToDoItemId"))
        {
            await bot.SendMessage(chatId, "Задача не указана.", replyMarkup: GetMainKeyboard(), cancellationToken: ct);
            return ScenarioResult.Completed;
        }

        var toDoItemId = (Guid)context.Data["ToDoItemId"];
        var task = await _toDoService.GetAsync(toDoItemId, ct);

        if (task == null)
        {
            await bot.SendMessage(chatId, "Задача не найдена.", replyMarkup: GetMainKeyboard(), cancellationToken: ct);
            return ScenarioResult.Completed;
        }

        context.Data["ToDoItem"] = task;
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
            $"Подтверждаете удаление задачи \"{task.Name}\"?",
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
            var task = (ToDoItem)context.Data["ToDoItem"];
            await _toDoService.DeleteAsync(task.Id, ct);

            await bot.SendMessage(
                chatId,
                $"Задача \"{task.Name}\" удалена.",
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
