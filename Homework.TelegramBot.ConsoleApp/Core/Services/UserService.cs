using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            var user = new ToDoUser(telegramUserId, telegramUserName);
            await _userRepository.AddAsync(user, ct);
            return user;
        }

        public Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken ct)
        {
            return _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct);
        }
    }
}
