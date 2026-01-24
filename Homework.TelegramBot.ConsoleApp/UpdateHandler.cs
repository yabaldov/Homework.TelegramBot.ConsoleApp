using System;
using System.Linq;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace Homework.TelegramBot.ConsoleApp
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            var chat = update.Message.Chat;
            var from = update.Message.From;
            var text = update.Message.Text?.Trim() ?? string.Empty;

            try
            {
                var user = _userService.GetUser(from.Id);

                switch (text)
                {
                    case "/start":
                        HandleStart(botClient, chat, from, user);
                        break;
                    case "/help":
                        HandleHelp(botClient, chat, user);
                        break;
                    case "/info":
                        HandleInfo(botClient, chat);
                        break;
                    case "/showtasks":
                        HandleShowTasks(botClient, chat, user);
                        break;
                    case "/showalltasks":
                        HandleShowAllTasks(botClient, chat, user);
                        break;
                    case string cmd when cmd.StartsWith("/addtask"):
                        HandleAddTask(botClient, chat, user, cmd);
                        break;
                    case string cmd when cmd.StartsWith("/removetask"):
                        HandleRemoveTask(botClient, chat, user, cmd);
                        break;
                    case string cmd when cmd.StartsWith("/completetask"):
                        HandleCompleteTask(botClient, chat, user, cmd);
                        break;
                    case "/exit":
                        HandleExit(botClient, chat);
                        break;
                    default:
                        botClient.SendMessage(chat, "Неизвестная команда. Введите /help для списка доступных команд.");
                        break;
                }
            }
            catch (Exception ex)
            {
                botClient.SendMessage(chat, $"Ошибка: {ex.Message}");
            }
        }

        private void HandleStart(ITelegramBotClient botClient, Chat chat, User from, ToDoUser? user)
        {
            if (user != null)
            {
                botClient.SendMessage(chat, $"Вы уже зарегистрированы как {user.TelegramUserName}.");
                return;
            }

            var userName = from.Username ?? $"User_{from.Id}";
            var newUser = _userService.RegisterUser(from.Id, userName);
            botClient.SendMessage(chat, $"Привет, {newUser.TelegramUserName}!");
            botClient.SendMessage(chat, "Теперь вам доступны команды: /addtask, /showtasks, /showalltasks, /removetask, /completetask, /exit");
        }

        private void HandleHelp(ITelegramBotClient botClient, Chat chat, ToDoUser? user)
        {
            var help = "Список доступных команд:\n" +
                       "/start -- начать работу (регистрация).\n" +
                       "/help -- показать эту справку.\n" +
                       "/info -- показать информацию о программе.\n";

            if (user != null)
            {
                help += "/addtask <название> -- добавить задачу.\n" +
                        "/showtasks -- показать активные задачи.\n" +
                        "/showalltasks -- показать все задачи.\n" +
                        "/completetask <Id> -- завершить задачу по Id.\n" +
                        "/removetask <номер> -- удалить задачу по номеру.\n" +
                        "/exit -- выйти из программы.";
            }

            botClient.SendMessage(chat, help);
        }

        private void HandleInfo(ITelegramBotClient botClient, Chat chat)
        {
            botClient.SendMessage(chat, "Программа: Симулятор бота Телеграм.\nВерсия: 0.0.4\nДата создания: 2025-08-22");
        }

        private void HandleShowTasks(ITelegramBotClient botClient, Chat chat, ToDoUser? user)
        {
            if (user == null)
            {
                botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.");
                return;
            }

            var tasks = _toDoService.GetActiveByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                botClient.SendMessage(chat, "Список задач пуст.");
                return;
            }

            var message = "Ваши задачи:\n";
            foreach (var task in tasks)
            {
                message += $"{task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
            }

            botClient.SendMessage(chat, message.TrimEnd());
        }

        private void HandleShowAllTasks(ITelegramBotClient botClient, Chat chat, ToDoUser? user)
        {
            if (user == null)
            {
                botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.");
                return;
            }

            var tasks = _toDoService.GetAllByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                botClient.SendMessage(chat, "Список задач пуст.");
                return;
            }

            var message = "Все задачи:\n";
            foreach (var task in tasks)
            {
                message += $"({task.State}) {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
            }

            botClient.SendMessage(chat, message.TrimEnd());
        }

        private void HandleAddTask(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command)
        {
            if (user == null)
            {
                botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.");
                return;
            }

            var taskName = command.Length > 8 ? command.Substring(8).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(taskName))
            {
                botClient.SendMessage(chat, "Пожалуйста, укажите название задачи. Пример: /addtask Купить молоко");
                return;
            }

            var task = _toDoService.Add(user, taskName);
            botClient.SendMessage(chat, $"Задача \"{task.Name}\" добавлена.");
        }

        private void HandleRemoveTask(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command)
        {
            if (user == null)
            {
                botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.");
                return;
            }

            var numberPart = command.Length > 11 ? command.Substring(11).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(numberPart))
            {
                var tasks = _toDoService.GetAllByUserId(user.UserId);
                if (tasks.Count == 0)
                {
                    botClient.SendMessage(chat, "Список задач пуст. Удаление невозможно.");
                    return;
                }

                var message = "Укажите номер задачи для удаления. Пример: /removetask 1\n\nВаши задачи:\n";
                for (int i = 0; i < tasks.Count; i++)
                {
                    message += $"{i + 1}. {tasks[i].Name}\n";
                }
                botClient.SendMessage(chat, message.TrimEnd());
                return;
            }

            if (!int.TryParse(numberPart, out int taskNumber))
            {
                botClient.SendMessage(chat, "Неверный формат номера задачи. Пример: /removetask 1");
                return;
            }

            var allTasks = _toDoService.GetAllByUserId(user.UserId);

            if (taskNumber < 1 || taskNumber > allTasks.Count)
            {
                botClient.SendMessage(chat, $"Неверный номер задачи. Доступные номера: 1-{allTasks.Count}");
                return;
            }

            var taskToRemove = allTasks[taskNumber - 1];
            _toDoService.Delete(taskToRemove.Id);
            botClient.SendMessage(chat, $"Задача \"{taskToRemove.Name}\" удалена.");
        }

        private void HandleCompleteTask(ITelegramBotClient botClient, Chat chat, ToDoUser? user, string command)
        {
            if (user == null)
            {
                botClient.SendMessage(chat, "Сначала используйте команду /start для регистрации.");
                return;
            }

            var idPart = command.Length > 13 ? command.Substring(13).Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(idPart))
            {
                botClient.SendMessage(chat, "Пожалуйста, укажите Id задачи. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e");
                return;
            }

            if (!Guid.TryParse(idPart, out Guid taskId))
            {
                botClient.SendMessage(chat, "Неверный формат Id. Пожалуйста, введите корректный GUID.");
                return;
            }

            var tasks = _toDoService.GetAllByUserId(user.UserId);
            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
            {
                botClient.SendMessage(chat, "Задача с указанным Id не найдена.");
                return;
            }

            _toDoService.MarkCompleted(taskId);
            botClient.SendMessage(chat, $"Задача \"{task.Name}\" завершена.");
        }
    
        private void HandleExit(ITelegramBotClient botClient, Chat chat)
        {
            botClient.SendMessage(chat, "Программа завершена.");
            Environment.Exit(0);
        }
    
    }
}
