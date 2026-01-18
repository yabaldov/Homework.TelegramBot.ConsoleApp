using System.Collections.Generic;

namespace Homework.TelegramBot.ConsoleApp
{

    public class UserData
    {
        private ToDoUser? _user;
        private int _tasksLimit;
        private int _taskLengthLimit;
        private List<string> _tasks;

        public UserData(ToDoUser user, int tasksLimit, int taskLengthLimit)
        {
            _user = user;
            _tasksLimit = tasksLimit;
            _taskLengthLimit = taskLengthLimit;
            _tasks = new List<string>();
        }

        public UserData(int tasksLimit, int taskLengthLimit)
        {
            _user = null;
            _tasksLimit = tasksLimit;
            _taskLengthLimit = taskLengthLimit;
            _tasks = new List<string>();
        }

        public UserData()
        {
            _user = null;
            _tasksLimit = 0;
            _taskLengthLimit = 0;
            _tasks = new List<string>();
        }

        public bool IsUserDataLimitsNotSet()
        {
            return _tasksLimit < 1 || _taskLengthLimit < 1;
        }

        public ToDoUser? User
        {
            get => _user;
            set => _user = value;
        }

        public int TasksLimit
        {
            get => _tasksLimit;
            set => _tasksLimit = value;
        }

        public int TaskLengthLimit
        {
            get => _taskLengthLimit;
            set => _taskLengthLimit = value;
        }

        public List<string> Tasks
        {
            get => _tasks;
            set => _tasks = value;
        }

    }

}
