using System.Collections.Generic;
using System.Linq;

namespace Homework.TelegramBot.ConsoleApp.Helpers
{
    public static class EnumerableExtension
    {
        public static IEnumerable<TSource> GetBatchByNumber<TSource>(
            this IEnumerable<TSource> source, int batchSize, int batchNumber)
        {
            return source.Skip(batchSize * batchNumber).Take(batchSize);
        }
    }
}
