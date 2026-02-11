using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

namespace Homework.TelegramBot.ConsoleApp.BackgroundTasks;

public class ResetScenarioBackgroundTask(
    TimeSpan resetScenarioTimeout,
    IScenarioContextRepository scenarioRepository,
    ITelegramBotClient bot)
    : BackgroundTask(TimeSpan.FromHours(1), nameof(ResetScenarioBackgroundTask))
{
    protected override async Task Execute(CancellationToken ct)
    {
        var contexts = await scenarioRepository.GetContexts(ct);

        foreach (var context in contexts)
        {
            if (DateTime.UtcNow - context.CreatedAt <= resetScenarioTimeout)
                continue;

            await scenarioRepository.ResetContext(context.UserId, ct);

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "/addtask", "/show", "/report" }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendMessage(
                context.UserId,
                $"Сценарий отменен, так как не поступил ответ в течение {resetScenarioTimeout}",
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}
