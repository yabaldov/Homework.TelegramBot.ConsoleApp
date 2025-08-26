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
                    if (userData.IsUserDataLimitsNotSet()) {
						Console.Write("Введите максимально допустимое количество задач (1-100): ");
						string? input = Console.ReadLine();
						userData.TasksLimit = StringValidator.ParseAndValidateInt(input, minTasksLimit, maxTasksLimit);

						Console.Write("Введите максимально допустимую длину задачи (1-100): ");
						input = Console.ReadLine();
						userData.TaskLengthLimit = StringValidator.ParseAndValidateInt(input, minTaskLength, maxTaskLength);

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