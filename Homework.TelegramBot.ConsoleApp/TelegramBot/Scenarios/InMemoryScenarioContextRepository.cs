using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Homework.TelegramBot.ConsoleApp.TelegramBot.Scenarios;

/// <summary>
/// In-memory реализация репозитория контекстов сценариев.
/// Хранит контексты в памяти с использованием Dictionary.
/// </summary>
public class InMemoryScenarioContextRepository : IScenarioContextRepository
{
    private readonly Dictionary<long, ScenarioContext> _contexts = new();

    public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
    {
        _contexts.TryGetValue(userId, out var context);
        return Task.FromResult(context);
    }

    public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
    {
        context.UserId = userId;
        _contexts[userId] = context;
        return Task.CompletedTask;
    }

    public Task ResetContext(long userId, CancellationToken ct)
    {
        _contexts.Remove(userId);
        return Task.CompletedTask;
    }
}
