using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Core.Services
{
    public interface IUserService
    {
        ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
        ToDoUser? GetUser(long telegramUserId);
    }
}
