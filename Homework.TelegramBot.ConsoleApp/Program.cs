using System;
using System.IO;
using Homework.TelegramBot.ConsoleApp;

namespace Homework.TelegramBot.ConsoleApp
{
	public static class Program
	{
		static void Main(string[] args)
		{


			const int minTasksLimit = 1;
			const int maxTasksLimit = 100;
			const int minTaskLength = 1;
			const int maxTaskLength = 100;

			bool hasUnexpectedError = false;

			var userData = new UserData();
			
			Console.WriteLine($"Добро пожаловать в симулятор бота Телеграм! {Environment.NewLine}");

			while (true)
			{
				try
				{
                    if (userData.TasksLimit < 1 || userData.TaskLengthLimit < 1) {
						Console.Write("Введите максимально допустимое количество задач: ");
						string? input = Console.ReadLine();
						if (!int.TryParse(input, out int tasksLimit))
						{
							Console.WriteLine($"Введено некорректное число: \"{input}\".");
							continue;
						}
						if (tasksLimit < minTasksLimit || tasksLimit > maxTasksLimit)
						{
							throw new ArgumentException($"Максимально допустимое количество задач должно быть в диапазоне от {minTasksLimit} до {maxTasksLimit}. Введено: {tasksLimit}");
						}

						Console.Write("Введите максимально допустимую длину задачи: ");
						input = Console.ReadLine();
						if (!int.TryParse(input, out int taskLengthLimit))
						{
							Console.WriteLine($"Введено некорректное число: \"{input}\".");
							continue;
						}
						if (taskLengthLimit < minTaskLength || taskLengthLimit > maxTaskLength)
						{
							throw new ArgumentException($"Максимально допустимая длина задачи должна быть в диапазоне от {minTasksLimit} до {maxTasksLimit}. Введено: {taskLengthLimit}");
						}

						userData.TasksLimit = tasksLimit;
						userData.TaskLengthLimit = taskLengthLimit;
					}

					var bot = new Bot(userData);
					bot.Run();
					break;
				}
				catch (ArgumentException ex)
				{
					Console.WriteLine($"Ошибка: {ex.Message}");
					continue;
				}
				catch (TaskCountLimitException ex)
				{
					Console.WriteLine($"Ошибка: {ex.Message}");
					continue;
				}
				catch (TaskLengthLimitException ex)
				{
					Console.WriteLine($"Ошибка: {ex.Message}");
					continue;
				}
				catch (DuplicateTaskException ex)
				{
					Console.WriteLine($"Ошибка: {ex.Message}");
					continue;
				}
				catch (Exception ex)
				{
					hasUnexpectedError = true;
					Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType().Name}");
					Console.WriteLine($"Описание ошибки: {ex.Message}");
					Console.WriteLine($"Трассировка стека:{Environment.NewLine}{ex.StackTrace}");
					if (ex.InnerException != null)
					{
						Console.WriteLine($"Внутреннее исключение:{Environment.NewLine}{ex.InnerException.Message}");
					}
					break;
				}
			}

			Console.WriteLine(hasUnexpectedError
				? "Программа завершена с ошибкой."
				: "Программа завершена успешно."
				);
			if (Environment.UserInteractive)
			{
				Console.WriteLine("Нажмите любую клавишу для выхода...");
				Console.ReadKey(true);
			}
		}
	}
}