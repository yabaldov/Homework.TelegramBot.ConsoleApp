using System.Collections.Generic;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

/// <summary>
/// Контекст (сессия) сценария пользователя.
/// Хранит информацию о текущем состоянии многошагового диалога.
/// </summary>
public class ScenarioContext
{
    /// <summary>
    /// Id пользователя в Telegram.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Текущий активный сценарий.
    /// </summary>
    public ScenarioType CurrentScenario { get; set; }

    /// <summary>
    /// Текущий шаг сценария.
    /// </summary>
    public string? CurrentStep { get; set; }

    /// <summary>
    /// Дополнительная информация, необходимая для работы сценария.
    /// </summary>
    public Dictionary<string, object> Data { get; set; }

    public ScenarioContext(ScenarioType scenario)
    {
        CurrentScenario = scenario;
        Data = new Dictionary<string, object>();
    }
}
