using System;
using LinqToDB.Mapping;

namespace Homework.TelegramBot.ConsoleApp.Core.DataAccess.Models
{
    [Table("Notifications")]
    public class NotificationModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("Type")]
        public string Type { get; set; } = string.Empty;

        [Column("Text")]
        public string Text { get; set; } = string.Empty;

        [Column("ScheduledAt")]
        public DateTime ScheduledAt { get; set; }

        [Column("IsNotified")]
        public bool IsNotified { get; set; }

        [Column("NotifiedAt")]
        public DateTime? NotifiedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; } = null!;
    }
}
