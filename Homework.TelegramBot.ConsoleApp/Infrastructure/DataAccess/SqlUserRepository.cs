using System;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using LinqToDB;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var model = await db.ToDoUsers
                .FirstOrDefaultAsync(u => u.UserId == userId, ct);
            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var model = await db.ToDoUsers
                .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);
            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(user);
            await db.InsertAsync(model, token: ct);
        }
    }
}
