using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Core.Services
{
    public class ToDoListService : IToDoListService
    {
        private const int MaxListNameLength = 10;
        private readonly IToDoListRepository _toDoListRepository;

        public ToDoListService(IToDoListRepository toDoListRepository)
        {
            _toDoListRepository = toDoListRepository;
        }

        public async Task<ToDoList> AddAsync(ToDoUser user, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Название списка не может отсутствовать.");
            }

            if (name.Length > MaxListNameLength)
            {
                throw new ArgumentException($"Название списка не может быть длиннее {MaxListNameLength} символов.");
            }

            if (await _toDoListRepository.ExistsByNameAsync(user.UserId, name, ct))
            {
                throw new ArgumentException($"Список с названием \"{name}\" уже существует.");
            }

            var list = new ToDoList
            {
                Id = Guid.NewGuid(),
                Name = name,
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            await _toDoListRepository.AddAsync(list, ct);
            return list;
        }

        public Task<ToDoList?> GetAsync(Guid id, CancellationToken ct)
        {
            return _toDoListRepository.GetAsync(id, ct);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            return _toDoListRepository.DeleteAsync(id, ct);
        }

        public Task<IReadOnlyList<ToDoList>> GetUserListsAsync(Guid userId, CancellationToken ct)
        {
            return _toDoListRepository.GetByUserIdAsync(userId, ct);
        }
    }
}
