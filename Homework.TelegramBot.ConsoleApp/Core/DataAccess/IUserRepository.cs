using System;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Core.DataAccess
{
    public interface IUserRepository
    {
        Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct);
        Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct);
        Task AddAsync(ToDoUser user, CancellationToken ct);
    }
}
