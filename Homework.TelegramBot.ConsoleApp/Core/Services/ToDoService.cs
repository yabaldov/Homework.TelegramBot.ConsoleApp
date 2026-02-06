using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Core.Exceptions;

namespace Homework.TelegramBot.ConsoleApp.Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;
        private readonly int _taskCountLimit;
        private readonly int _taskLengthLimit;

        public ToDoService(IToDoRepository toDoRepository, int taskCountLimit, int taskLengthLimit)
        {
            _toDoRepository = toDoRepository;
            _taskCountLimit = taskCountLimit;
            _taskLengthLimit = taskLengthLimit;
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return _toDoRepository.GetAllByUserIdAsync(userId, ct);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return _toDoRepository.GetActiveByUserIdAsync(userId, ct);
        }

        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct)
        {
            if (await _toDoRepository.CountActiveAsync(user.UserId, ct) >= _taskCountLimit)
            {
                throw new TaskCountLimitException(_taskCountLimit);
            }

            if (name.Length > _taskLengthLimit)
            {
                throw new TaskLengthLimitException(name.Length, _taskLengthLimit);
            }

            if (await _toDoRepository.ExistsByNameAsync(user.UserId, name, ct))
            {
                throw new DuplicateTaskException(name);
            }

            var task = new ToDoItem(user, name, deadline, list);
            await _toDoRepository.AddAsync(task, ct);
            return task;
        }

        public async Task MarkCompletedAsync(Guid id, CancellationToken ct)
        {
            var task = await _toDoRepository.GetAsync(id, ct);
            if (task != null)
            {
                task.State = ToDoItemState.Completed;
                task.StateChangedAt = DateTime.UtcNow;
                await _toDoRepository.UpdateAsync(task, ct);
            }
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            return _toDoRepository.DeleteAsync(id, ct);
        }

        public Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            return _toDoRepository.FindAsync(user.UserId, item => item.Name.StartsWith(namePrefix), ct);
        }

        public Task<IReadOnlyList<ToDoItem>> GetByUserIdAndListAsync(Guid userId, Guid? listId, CancellationToken ct)
        {
            return _toDoRepository.GetByUserIdAndListAsync(userId, listId, ct);
        }

        public Task<ToDoItem?> GetAsync(Guid toDoItemId, CancellationToken ct)
        {
            return _toDoRepository.GetAsync(toDoItemId, ct);
        }
    }
}
