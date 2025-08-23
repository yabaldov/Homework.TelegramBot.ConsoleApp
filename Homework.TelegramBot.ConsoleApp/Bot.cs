using System;

namespace Homework.TelegramBot.ConsoleApp
{
    public class Bot
    {
        private string _userName;
        private bool _isRunning;
        private readonly Tasker _tasker;
        private UserData _userData;
        
        public Bot(UserData userData)
        {
            _isRunning = true;
            _userName = userData.UserName;
            _tasker = new Tasker(userData.Tasks, userData.TasksLimit, userData.TaskLengthLimit);
            _userData = userData;
        }

        public void Run()
        {
            Console.WriteLine("----------------------------------------------------------------");
            if (string.IsNullOrEmpty(_userName))
            {
                Console.WriteLine("Доступные команды: /start, /help, /info, /exit");
                Console.WriteLine("Пожалуйста, сначала используйте команду /start для ввода вашего имени.");
            }
            else
            {
                Console.WriteLine($"{_userName}!");
                Console.WriteLine($"Ваш лимит задач: {_userData.TasksLimit}, текущие задачи: {_userData.Tasks.Count}.");
                Console.WriteLine($"Ваш лимит длины задачи: {_userData.TaskLengthLimit}.");
                Console.WriteLine("Доступные команды: /start, /help, /info, /exit, /echo, /addtask, /showtasks, /removetask");
            }

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
            if (string.IsNullOrEmpty(_userName))
            {
                Console.WriteLine("Имя не может быть пустым. Пожалуйста, попробуйте снова.");
                return;
            }
            _userData.UserName = _userName.Trim();
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
            Console.WriteLine("Версия: 0.0.3");
            Console.WriteLine("Дата создания: 2025-08-22");
        }

        private void Echo(string command)
        {
            if (string.IsNullOrEmpty(_userName))
            {
                Console.WriteLine("Сначала используйте команду /start и введите своё имя.");
                return;
            }

            string echoText = command.IndexOf(' ') > 0 ? command.Substring(6).Trim() : String.Empty;
            if (!string.IsNullOrEmpty(echoText))
            {
                Console.WriteLine($"{_userName}, вы написали: {echoText}");
            }
            else
            {
                Console.WriteLine("Пожалуйста, добавьте пробел и текст после команды /echo.");
            }
        }

        private void Exit()
        {
            Console.WriteLine("Выход из программы. Пока!");
            _isRunning = false;
        }
    }
}