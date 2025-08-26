using System.Collections.Generic;

namespace Homework.TelegramBot.ConsoleApp
{

    public class UserData
    {
        private string _userName;
        private int _tasksLimit;
        private int _taskLengthLimit;
        private List<string> _tasks;

        public UserData(string userName, int tasksLimit, int taskLengthLimit)
        {
            _userName = userName;
            _tasksLimit = tasksLimit;
            _taskLengthLimit = taskLengthLimit;
            _tasks = new List<string>();
        }

        public UserData()
        {
            _userName = string.Empty;
            _tasksLimit = 0;
            _taskLengthLimit = 0;
            _tasks = new List<string>();
        }

        public bool IsUserDataLimitsNotSet()
        {
            return _tasksLimit < 1 || _taskLengthLimit < 1;
        }

        public string UserName
        {
            get => _userName;
            set => _userName = value;
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
