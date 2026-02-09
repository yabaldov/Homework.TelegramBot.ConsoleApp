using System;
using LinqToDB.Mapping;

namespace Homework.TelegramBot.ConsoleApp.Core.DataAccess.Models
{
    [Table("ToDoUsers")]
    public class ToDoUserModel
    {
        [PrimaryKey]
        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("TelegramUserId")]
        public long TelegramUserId { get; set; }

        [Column("TelegramUserName")]
        public string TelegramUserName { get; set; } = string.Empty;

        [Column("RegisteredAt")]
        public DateTime RegisteredAt { get; set; }
    }
}
