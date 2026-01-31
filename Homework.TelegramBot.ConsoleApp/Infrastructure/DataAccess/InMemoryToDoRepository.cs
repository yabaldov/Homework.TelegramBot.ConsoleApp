using System;
using System.Collections.Generic;
using System.Linq;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _items = new();

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _items.Where(t => t.User.UserId == userId).ToList();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _items.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
        }

        public ToDoItem? Get(Guid id)
        {
            return _items.FirstOrDefault(t => t.Id == id);
        }

        public void Add(ToDoItem item)
        {
            _items.Add(item);
        }

        public void Update(ToDoItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var i = _items.FindIndex(t => t.Id == item.Id);
            if (i == -1)
            {
                throw new InvalidOperationException($"Задача с Id {item.Id} не найдена в памяти.");
            }

            _items[i] = item;
        }

        public void Delete(Guid id)
        {
            var item = _items.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public bool ExistsByName(Guid userId, string name)
        {
            return _items.Any(t => t.User.UserId == userId && t.Name == name);
        }

        public int CountActive(Guid userId)
        {
            return _items.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return _items.Where(t => t.User.UserId == userId && predicate(t)).ToList();
        }
    }
}
