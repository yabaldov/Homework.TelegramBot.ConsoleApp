namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

/// <summary>
/// Типы поддерживаемых сценариев.
/// </summary>
public enum ScenarioType
{
    /// <summary>
    /// Нет активного сценария.
    /// </summary>
    None,

    /// <summary>
    /// Сценарий добавления задачи.
    /// </summary>
    AddTask,

    /// <summary>
    /// Сценарий добавления списка.
    /// </summary>
    AddList,

    /// <summary>
    /// Сценарий удаления списка.
    /// </summary>
    DeleteList,

    /// <summary>
    /// Сценарий удаления задачи.
    /// </summary>
    DeleteTask
}
