using System;
using Homework.TelegramBot.ConsoleApp.Core.Entities;

namespace Homework.TelegramBot.ConsoleApp.Core.DataAccess
{
    public interface IUserRepository
    {
        ToDoUser? GetUser(Guid userId);
        ToDoUser? GetUserByTelegramUserId(long telegramUserId);
        void Add(ToDoUser user);
    }
}
