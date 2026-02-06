using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Services;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

public class AddListScenario : IScenario
{
    private readonly IUserService _userService;
    private readonly IToDoListService _toDoListService;

    public AddListScenario(IUserService userService, IToDoListService toDoListService)
    {
        _userService = userService;
        _toDoListService = toDoListService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.AddList;
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
            "Введите название списка:",
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
                "Название списка не может быть пустым. Введите название списка:",
                replyMarkup: GetCancelKeyboard(),
                cancellationToken: ct);
            return ScenarioResult.Transition;
        }

        var user = (ToDoUser)context.Data["User"];
        var list = await _toDoListService.AddAsync(user, name, ct);

        await bot.SendMessage(
            chatId,
            $"Список \"{list.Name}\" создан.",
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
