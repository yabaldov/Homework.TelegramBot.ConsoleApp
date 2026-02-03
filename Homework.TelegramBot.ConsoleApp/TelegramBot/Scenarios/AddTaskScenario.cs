using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Services;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

public class AddTaskScenario : IScenario
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;

    public AddTaskScenario(IUserService userService, IToDoService toDoService)
    {
        _userService = userService;
        _toDoService = toDoService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.AddTask;
    }

    public async Task<ScenarioResult> HandleMessageAsync(
        ITelegramBotClient bot,
        ScenarioContext context,
        Update update,
        CancellationToken ct
        )
    {
        var chat = update.Message!.Chat;
        var text = update.Message.Text?.Trim() ?? string.Empty;

        switch (context.CurrentStep)
        {
            case null:
                return await HandleStartStepAsync(bot, chat, context, ct);

            case "Name":
                return await HandleNameStepAsync(bot, chat, context, text, ct);

            default:
                return ScenarioResult.Completed;
        }
    }

    private async Task<ScenarioResult> HandleStartStepAsync(
        ITelegramBotClient bot,
        Chat chat,
        ScenarioContext context,
        CancellationToken ct)
    {
        var user = await _userService.GetUserAsync(context.UserId, ct);
        if (user == null)
        {
            await bot.SendMessage(chat.Id, "Пользователь не найден. Используйте /start для регистрации.", cancellationToken: ct);
            return ScenarioResult.Completed;
        }

        context.Data["User"] = user;
        context.CurrentStep = "Name";

        await bot.SendMessage(
            chat.Id,
            "Введите название задачи:",
            replyMarkup: GetCancelKeyboard(),
            cancellationToken: ct);

        return ScenarioResult.Transition;
    }

    private async Task<ScenarioResult> HandleNameStepAsync(
        ITelegramBotClient bot,
        Chat chat,
        ScenarioContext context,
        string name,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            await bot.SendMessage(
                chat.Id,
                "Название задачи не может быть пустым. Введите название задачи:",
                replyMarkup: GetCancelKeyboard(),
                cancellationToken: ct);
            return ScenarioResult.Transition;
        }

        var user = (ToDoUser)context.Data["User"];
        var task = await _toDoService.AddAsync(user, name, ct);

        await bot.SendMessage(
            chat.Id,
            $"Задача \"{task.Name}\" добавлена.",
            replyMarkup: GetMainKeyboard(),
            cancellationToken: ct);

        return ScenarioResult.Completed;
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
            new KeyboardButton[] { "/addtask", "/showtasks", "/showalltasks", "/report" }
        })
        {
            ResizeKeyboard = true
        };
    }
}
