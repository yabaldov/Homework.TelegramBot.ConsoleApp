using System;

namespace Homework.TelegramBot.ConsoleApp
{
    public class Bot
    {
        private ToDoUser? _user;
        private bool _isRunning;
        private readonly Tasker _tasker;
        private UserData _userData;

        public Bot(UserData userData)
        {
            _isRunning = true;
            _user = userData.User;
            _tasker = new Tasker(userData.Tasks, userData.TasksLimit, userData.TaskLengthLimit);
            _userData = userData;
        }

        public void Run()
        {
            Console.WriteLine(new string('-', 80));
            if (_user == null)
            {
                Console.WriteLine("Доступные команды: /start, /help, /info, /exit");
                Console.WriteLine("Пожалуйста, сначала используйте команду /start для ввода вашего имени.");
            }
            else
            {
                Console.WriteLine($"{_user.TelegramUserName}!");
                Console.WriteLine($"Ваш лимит задач: {_userData.TasksLimit}, текущие задачи: {_userData.Tasks.Count}.");
                Console.WriteLine($"Ваш лимит длины задачи: {_userData.TaskLengthLimit}.");
                Console.WriteLine("Доступные команды: /start, /help, /info, /exit, /echo, /addtask, /showtasks, /removetask");
            }

            while (_isRunning)
            {
                Console.Write("Введите команду: ");
                string? input = Console.ReadLine();
                StringValidator.ValidateString(input);
                string command = input!.Trim();

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
                        if (_user == null)
                        {
                            Console.WriteLine("Сначала используйте команду /start и введите своё имя.");
                        }
                        else
                        {
                            _tasker.AddTask(_user);
                        }
                        break;
                    case "/showtasks":
                        _tasker.ShowTasks();
                        break;
                    case "/showalltasks":
                        _tasker.ShowAllTasks();
                        break;
                    case "/removetask":
                        _tasker.RemoveTask();
                        break;
                    case string completeCommand when completeCommand.StartsWith("/completetask"):
                        CompleteTask(completeCommand);
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
            Console.Write("Пожалуйста, введите ваше имя: ");
            string? input = Console.ReadLine();
            StringValidator.ValidateString(input);
            _user = new ToDoUser(input!.Trim());
            _userData.User = _user;

            Console.WriteLine($"Привет, {_user.TelegramUserName}!");
            Console.WriteLine("Теперь вы ещё можете использовать команды: /echo, /addtask, /showtasks, /removetask");
        }

        private void ShowHelp()
        {
            Console.WriteLine("Список доступных команд:");
            Console.WriteLine("/start -- начать работу и ввести своё имя.");
            Console.WriteLine("/help -- показать эту справку.");
            Console.WriteLine("/info -- показать информацию о программе.");
            if (_user != null)
            {
                Console.WriteLine("/echo [текст] -- повторить введённый текст.");
                Console.WriteLine("/addtask -- добавить задачу в список.");
                Console.WriteLine("/showtasks -- показать активные задачи.");
                Console.WriteLine("/showalltasks -- показать все задачи.");
                Console.WriteLine("/completetask [Id] -- завершить задачу по Id.");
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
            if (_user == null)
            {
                Console.WriteLine("Сначала используйте команду /start и введите своё имя.");
                return;
            }

            string echoText = command.IndexOf(' ') > 0 ? command.Substring(6).Trim() : String.Empty;
            if (!string.IsNullOrEmpty(echoText))
            {
                Console.WriteLine($"{_user.TelegramUserName}, вы написали: {echoText}");
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

        private void CompleteTask(string command)
        {
            string idPart = command.Length > 13 ? command.Substring(13).Trim() : string.Empty;

            if (string.IsNullOrEmpty(idPart))
            {
                Console.WriteLine("Пожалуйста, укажите Id задачи. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e");
                return;
            }

            if (Guid.TryParse(idPart, out Guid taskId))
            {
                _tasker.CompleteTask(taskId);
            }
            else
            {
                Console.WriteLine("Неверный формат Id. Пожалуйста, введите корректный GUID.");
            }
        }
    }
}
