using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using LinqToDB;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess
{
    public class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<ToDoList?> GetAsync(Guid id, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var model = await db.ToDoLists
                .LoadWith(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id, ct);
            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var models = await db.ToDoLists
                .LoadWith(l => l.User)
                .Where(l => l.UserId == userId)
                .ToListAsync(ct);
            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task AddAsync(ToDoList list, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(list);
            await db.InsertAsync(model, token: ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            await db.ToDoLists
                .Where(l => l.Id == id)
                .DeleteAsync(ct);
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            return await db.ToDoLists
                .AnyAsync(l => l.UserId == userId && l.Name == name, ct);
        }
    }
}
