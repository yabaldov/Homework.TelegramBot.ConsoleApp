using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Core.DataAccess
{
    public interface IToDoListRepository
    {
        Task<ToDoList?> GetAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoList>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        Task AddAsync(ToDoList list, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct);
    }
}
