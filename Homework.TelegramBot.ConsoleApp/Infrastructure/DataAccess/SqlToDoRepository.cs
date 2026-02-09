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
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        private IQueryable<Core.DataAccess.Models.ToDoItemModel> ItemsWithIncludes(ToDoDataContext db)
        {
            return db.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var models = await ItemsWithIncludes(db)
                .Where(i => i.UserId == userId)
                .ToListAsync(ct);
            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var models = await ItemsWithIncludes(db)
                .Where(i => i.UserId == userId && i.State == ToDoItemState.Active)
                .ToListAsync(ct);
            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var model = await ItemsWithIncludes(db)
                .FirstOrDefaultAsync(i => i.Id == id, ct);
            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(item);
            await db.InsertAsync(model, token: ct);
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(item);
            await db.UpdateAsync(model, token: ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            await db.ToDoItems
                .Where(i => i.Id == id)
                .DeleteAsync(ct);
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            return await db.ToDoItems
                .AnyAsync(i => i.UserId == userId && i.Name == name, ct);
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            return await db.ToDoItems
                .CountAsync(i => i.UserId == userId && i.State == ToDoItemState.Active, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var models = await ItemsWithIncludes(db)
                .Where(i => i.UserId == userId)
                .ToListAsync(ct);
            return models
                .Select(ModelMapper.MapFromModel)
                .Where(predicate)
                .ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndListAsync(Guid userId, Guid? listId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var models = await ItemsWithIncludes(db)
                .Where(i => i.UserId == userId && i.ListId == listId)
                .ToListAsync(ct);
            return models.Select(ModelMapper.MapFromModel).ToList();
        }
    }
}
