using System.Threading;
using System.Threading.Tasks;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

/// <summary>
/// Репозиторий, который отвечает за доступ к контекстам пользователей.
/// </summary>
public interface IScenarioContextRepository
{
    /// <summary>
    /// Получить контекст пользователя.
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Контекст сценария или null, если сценарий не активен.</returns>
    Task<ScenarioContext?> GetContext(long userId, CancellationToken ct);

    /// <summary>
    /// Задать контекст пользователя.
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram.</param>
    /// <param name="context">Контекст сценария.</param>
    /// <param name="ct">Токен отмены.</param>
    Task SetContext(long userId, ScenarioContext context, CancellationToken ct);

    /// <summary>
    /// Сбросить (очистить) контекст пользователя.
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram.</param>
    /// <param name="ct">Токен отмены.</param>
    Task ResetContext(long userId, CancellationToken ct);
}
