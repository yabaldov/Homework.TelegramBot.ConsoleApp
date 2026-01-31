using System;
using System.Collections.Generic;
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

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_toDoRepository.CountActive(user.UserId) >= _taskCountLimit)
            {
                throw new TaskCountLimitException(_taskCountLimit);
            }

            if (name.Length > _taskLengthLimit)
            {
                throw new TaskLengthLimitException(name.Length, _taskLengthLimit);
            }

            if (_toDoRepository.ExistsByName(user.UserId, name))
            {
                throw new DuplicateTaskException(name);
            }

            var task = new ToDoItem(user, name);
            _toDoRepository.Add(task);
            return task;
        }

        public void MarkCompleted(Guid id)
        {
            var task = _toDoRepository.Get(id);
            if (task != null)
            {
                task.State = ToDoItemState.Completed;
                task.StateChangedAt = DateTime.UtcNow;
                _toDoRepository.Update(task);
            }
        }

        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _toDoRepository.Find(user.UserId, item => item.Name.StartsWith(namePrefix));
        }
    }
}
