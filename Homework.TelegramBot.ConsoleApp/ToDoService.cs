using System;
using System.Collections.Generic;
using System.Linq;

namespace Homework.TelegramBot.ConsoleApp
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _tasks = new();
        private readonly int _taskCountLimit;
        private readonly int _taskLengthLimit;

        public ToDoService(int taskCountLimit, int taskLengthLimit)
        {
            _taskCountLimit = taskCountLimit;
            _taskLengthLimit = taskLengthLimit;
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId).ToList();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            var userTasks = _tasks.Where(t => t.User.UserId == user.UserId).ToList();

            if (userTasks.Count >= _taskCountLimit)
            {
                throw new TaskCountLimitException(_taskCountLimit);
            }

            if (name.Length > _taskLengthLimit)
            {
                throw new TaskLengthLimitException(name.Length, _taskLengthLimit);
            }

            if (userTasks.Any(t => t.Name == name))
            {
                throw new DuplicateTaskException(name);
            }

            var task = new ToDoItem(user, name);
            _tasks.Add(task);
            return task;
        }

        public void MarkCompleted(Guid id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                task.State = ToDoItemState.Completed;
                task.StateChangedAt = DateTime.UtcNow;
            }
        }

        public void Delete(Guid id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                _tasks.Remove(task);
            }
        }
    }
}
