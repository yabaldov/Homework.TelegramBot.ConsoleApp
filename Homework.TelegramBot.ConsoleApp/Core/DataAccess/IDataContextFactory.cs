using LinqToDB.Data;

namespace Homework.TelegramBot.ConsoleApp.Core.DataAccess
{
    public interface IDataContextFactory<TDataContext> where TDataContext : DataConnection
    {
        TDataContext CreateDataContext();
    }
}
