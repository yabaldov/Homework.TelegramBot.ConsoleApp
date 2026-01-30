using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _toDoRepository;

        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken ct)
        {
            var allTasks = await _toDoRepository.GetAllByUserIdAsync(userId, ct);

            int total = allTasks.Count;
            int completed = allTasks.Count(t => t.State == ToDoItemState.Completed);
            int active = allTasks.Count(t => t.State == ToDoItemState.Active);

            return (total, completed, active, DateTime.UtcNow);
        }
    }
}
