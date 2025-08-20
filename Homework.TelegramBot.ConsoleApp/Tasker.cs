using System;
using System.Collections.Generic;

namespace Homework.TelegramBot.ConsoleApp
{
	public class Tasker
	{
		private List<string> _tasks;

		public Tasker()
		{
			_tasks = new List<string>();
		}

		public void AddTask()
		{
			Console.Write("Пожалуйста, введите описание задачи: ");
			string task = Console.ReadLine();
			_tasks.Add(task);
			Console.WriteLine($"Задача \"{task}\" добавлена.");
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
				Console.WriteLine($"{i + 1}. {_tasks[i]}");
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
				Console.WriteLine($"{i + 1}. {_tasks[i]}");
			}

			Console.Write("Введите номер задачи для удаления: ");
			if (int.TryParse(Console.ReadLine(), out int taskNumber) && taskNumber >= 1 && taskNumber <= _tasks.Count)
			{
				string removedTask = _tasks[taskNumber - 1];
				_tasks.RemoveAt(taskNumber - 1);
				Console.WriteLine($"Задача \"{removedTask}\" удалена.");
			}
			else
			{
				Console.WriteLine("Неверный номер задачи. Попробуйте ещё раз.");
			}
		}
	}
}