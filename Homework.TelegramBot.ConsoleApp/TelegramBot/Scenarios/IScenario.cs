using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

public interface IScenario
{
    bool CanHandle(ScenarioType scenario);

    Task<ScenarioResult> HandleMessageAsync(
        ITelegramBotClient bot,
        ScenarioContext context,
        Update update,
        CancellationToken ct
        );
}
