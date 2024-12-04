using System;

namespace Homework.TelegramBot.ConsoleApp
{
    public class Bot
    {
        private string _userName;
        private bool _isRunning;
        private readonly Tasker _tasker;

        public Bot()
        {
            _userName = string.Empty;
            _isRunning = true;
            _tasker = new Tasker();
        }

        public void Run()
        {
            Console.WriteLine("Добро пожаловать в симулятор бота Телеграм!");
            Console.WriteLine("Доступные команды: /start, /help, /info, /exit");

            while (_isRunning)
            {
                Console.Write("\nВведите команду: ");
                string command = Console.ReadLine();

                switch (command)
                {
                    case "/start":
                        Start();
                        break;
                    case "/help":
                        ShowHelp();
                        break;
                    case "/info":
                        ShowInfo();
                        break;
                    case string echoCommand when echoCommand.StartsWith("/echo"):
                        Echo(echoCommand);
                        break;
                    case "/addtask":
                        _tasker.AddTask();
                        break;
                    case "/showtasks":
                        _tasker.ShowTasks();
                        break;
                    case "/removetask":
                        _tasker.RemoveTask();
                        break;
                    case "/exit":
                        Exit();
                        break;
                    default:
                        Console.WriteLine("Неизвестная команда. Введите /help для списка доступных команд.");
                        break;
                }
            }
        }

        private void Start()
        {
            Console.Write("Пожалуйста, введите ваше Введите своё имя: ");
            _userName = Console.ReadLine();
            Console.WriteLine($"Привет, {_userName}!");
            Console.WriteLine("Теперь вы ещё можете использовать команды: /echo, /addtask, /showtask, /rermovetask");
        }

        private void ShowHelp()
        {
            Console.WriteLine("Список доступных команд:");
            Console.WriteLine("/start -- начать работу и ввести своё имя.");
            Console.WriteLine("/help -- показать эту справку.");
            Console.WriteLine("/info -- показать информацию о программе.");
            if (!string.IsNullOrEmpty(_userName))
            {
                Console.WriteLine("/echo [текст] -- повторить введённый текст.");
                Console.WriteLine("/addtask -- добавить задачу в список.");
                Console.WriteLine("/showtasks -- показать все задачи.");
                Console.WriteLine("/removetask -- удалить задачу из списка.");
            }
            Console.WriteLine("/exit -- выйти из программы.");
        }

        private void ShowInfo()
        {
            Console.WriteLine("Программа: Симулятор бота Телеграм.");
            Console.WriteLine("Версия: 0.0.2");
            Console.WriteLine("Дата создания: 2024-12-04");
        }

        private void Echo(string command)
        {
            if (string.IsNullOrEmpty(_userName))
            {
                Console.WriteLine("Сначала используйте команду /start и введите своё имя.");
                return;
            }

            string echoText = command.Substring(6).Trim();
            if (!string.IsNullOrEmpty(echoText))
            {
                Console.WriteLine($"{_userName}, вы написали: {echoText}");
            }
            else
            {
                Console.WriteLine("Пожалуйста, добавте текст после команды /echo.");
            }
        }

        private void Exit()
        {
            Console.WriteLine("Выход из программы. Пока!");
            _isRunning = false;
        }
    }
}