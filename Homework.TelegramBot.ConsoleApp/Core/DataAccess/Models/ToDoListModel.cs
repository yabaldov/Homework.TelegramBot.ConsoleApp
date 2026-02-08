using System;
using LinqToDB.Mapping;

namespace Homework.TelegramBot.ConsoleApp.Core.DataAccess.Models
{
    [Table("ToDoLists")]
    public class ToDoListModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; } = null!;
    }
}
