using System.Threading;
using System.Threading.Tasks;

namespace Homework.TelegramBot.ConsoleApp.BackgroundTasks;

public interface IBackgroundTask
{
    Task Start(CancellationToken ct);
}
