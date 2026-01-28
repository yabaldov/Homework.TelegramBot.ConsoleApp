using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _items = new();

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var result = _items.Where(t => t.User.UserId == userId).ToList();
            return Task.FromResult<IReadOnlyList<ToDoItem>>(result);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var result = _items.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
            return Task.FromResult<IReadOnlyList<ToDoItem>>(result);
        }

        public Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            var item = _items.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(item);
        }

        public Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            _items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            // В in-memory реализации объект уже обновлён по ссылке,
            // но метод нужен для совместимости с интерфейсом
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var item = _items.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                _items.Remove(item);
            }
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            var exists = _items.Any(t => t.User.UserId == userId && t.Name == name);
            return Task.FromResult(exists);
        }

        public Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            var count = _items.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
            return Task.FromResult(count);
        }

        public Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var result = _items.Where(t => t.User.UserId == userId && predicate(t)).ToList();
            return Task.FromResult<IReadOnlyList<ToDoItem>>(result);
        }
    }
}
