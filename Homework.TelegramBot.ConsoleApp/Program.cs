using System;
using System.IO;
using Homework.TelegramBot.ConsoleApp;

namespace Homework.TelegramBot.ConsoleApp
{
	public static class Program
	{
		static void Main(string[] args)
		{
			bool hasError = false;
			try
			{
				var bot = new Bot();
				bot.Run();

			}
			catch (Exception ex)
			{
				hasError = true;
				Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType().Name}");
				Console.WriteLine($"Описание ошибки: {ex.Message}");
				Console.WriteLine($"Трассировка стека:{Environment.NewLine}{ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Console.WriteLine($"Внутреннее исключение:{Environment.NewLine}{ex.InnerException.Message}");
				}

			}
			finally
			{
				if (hasError)
				{
					Console.WriteLine("Программа завершена с ошибкой. Нажмите любую клавишу для выхода.");
				}
				else
				{
					Console.WriteLine("Программа завершена успешно. Нажмите любую клавишу для выхода.");
				}
				Console.ReadKey(true);
			}	
		}
	}
}