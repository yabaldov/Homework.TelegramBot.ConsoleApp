using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            return Task.FromResult(user);
        }

        public Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            var user = _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
            return Task.FromResult(user);
        }

        public Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<ToDoUser>>(_users.ToList());
        }
    }
}
