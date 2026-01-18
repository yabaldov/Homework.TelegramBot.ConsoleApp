using System;
using System.Collections.Generic;
using System.Linq;

namespace Homework.TelegramBot.ConsoleApp
{
	public class Tasker
	{
		private List<ToDoItem> _tasks;
		private int _taskCountLimit;
		private int _taskLengthLimit;

		public Tasker(List<ToDoItem> tasks, int taskCountLimit, int taskLengthLimit)
		{
			_tasks = tasks;
			_taskCountLimit = taskCountLimit;
			_taskLengthLimit = taskLengthLimit;
		}

		public void AddTask(ToDoUser user)
		{
			if (_tasks.Count >= _taskCountLimit)
			{
				throw new TaskCountLimitException(_taskCountLimit);
			}

			Console.Write("Пожалуйста, введите описание задачи: ");
			string? input = Console.ReadLine();
			string taskDescription = input?.Trim() ?? string.Empty;

			if (string.IsNullOrWhiteSpace(taskDescription))
			{
				Console.WriteLine("Ошибка: Описание задачи не может быть пустым!");
				return;
			}

			if (taskDescription.Length > _taskLengthLimit)
			{
				throw new TaskLengthLimitException(taskDescription.Length, _taskLengthLimit);
			}

			if (_tasks.Any(t => t.Name == taskDescription))
			{
				throw new DuplicateTaskException(taskDescription);
			}

			var task = new ToDoItem(user, taskDescription);
			_tasks.Add(task);
			Console.WriteLine($"Задача \"{taskDescription}\" добавлена.");
		}

		public void ShowTasks()
		{
			if (_tasks.Count == 0)
			{
				Console.WriteLine("Список задач пуст.");
				return;
			}

			Console.WriteLine("Ваши задачи:");
			for (int i = 0; i < _tasks.Count; i++)
			{
				Console.WriteLine($"{i + 1}. {_tasks[i].Name}");
			}
		}

		public void RemoveTask()
		{
			if (_tasks.Count == 0)
			{
				Console.WriteLine("Список задач пуст. Удаление невозможно.");
				return;
			}

			Console.WriteLine("Вот ваш список задач:");
			for (int i = 0; i < _tasks.Count; i++)
			{
				Console.WriteLine($"{i + 1}. {_tasks[i].Name}");
			}

			Console.Write("Введите номер задачи для удаления: ");
			if (int.TryParse(Console.ReadLine(), out int taskNumber) && taskNumber >= 1 && taskNumber <= _tasks.Count)
			{
				string removedTaskName = _tasks[taskNumber - 1].Name;
				_tasks.RemoveAt(taskNumber - 1);
				Console.WriteLine($"Задача \"{removedTaskName}\" удалена.");
			}
			else
			{
				Console.WriteLine("Неверный номер задачи. Попробуйте ещё раз.");
			}
		}
	}
}
