using System;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using LinqToDB.Mapping;

namespace Homework.TelegramBot.ConsoleApp.Core.DataAccess.Models
{
    [Table("ToDoItems")]
    public class ToDoItemModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("ListId")]
        public Guid? ListId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("Deadline")]
        public DateTime Deadline { get; set; }

        [Column("State")]
        public ToDoItemState State { get; set; }

        [Column("StateChangedAt")]
        public DateTime? StateChangedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; } = null!;

        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id), CanBeNull = true)]
        public ToDoListModel? List { get; set; }
    }
}
