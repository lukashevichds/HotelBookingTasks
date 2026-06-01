using System.Collections.ObjectModel;
using System.Linq;
using HotelBookingTasks.Helpers;
using HotelBookingTasks.Models;
using HotelBookingTasks.Services;

namespace HotelBookingTasks.ViewModels
{
    public class TasksViewModel : BaseViewModel
    {
        private readonly TaskService _taskService;

        public ObservableCollection<TaskItem> Tasks { get; }

        private TaskItem? _selectedTask;
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (SetProperty(ref _selectedTask, value))
                {
                    MarkCompletedCommand.RaiseCanExecuteChanged();
                    DeleteTaskCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private int _totalTasks;
        public int TotalTasks
        {
            get => _totalTasks;
            set => SetProperty(ref _totalTasks, value);
        }

        private int _activeTasks;
        public int ActiveTasks
        {
            get => _activeTasks;
            set => SetProperty(ref _activeTasks, value);
        }

        private int _overdueTasks;
        public int OverdueTasks
        {
            get => _overdueTasks;
            set => SetProperty(ref _overdueTasks, value);
        }

        public RelayCommand LoadTasksCommand { get; }
        public RelayCommand MarkCompletedCommand { get; }
        public RelayCommand DeleteTaskCommand { get; }
        public RelayCommand RefreshExpiredTasksCommand { get; }

        public TasksViewModel()
        {
            _taskService = new TaskService();
            Tasks = new ObservableCollection<TaskItem>();

            LoadTasksCommand = new RelayCommand(_ => LoadTasks());
            MarkCompletedCommand = new RelayCommand(_ => MarkCompleted(), _ => SelectedTask != null);
            DeleteTaskCommand = new RelayCommand(_ => DeleteTask(), _ => SelectedTask != null);
            RefreshExpiredTasksCommand = new RelayCommand(_ => RefreshExpiredTasks());

            LoadTasks();
        }

        public void LoadTasks()
        {
            _taskService.UpdateExpiredTasks();
            Tasks.Clear();

            foreach (var task in _taskService.GetTasksForCurrentUser())
            {
                Tasks.Add(task);
            }

            TotalTasks = Tasks.Count;
            OverdueTasks = Tasks.Count(item => item.IsOverdue);
            ActiveTasks = Tasks.Count(item => !item.IsCompleted && item.Status != TaskItemStatus.Cancelled);
        }

        private void MarkCompleted()
        {
            if (SelectedTask == null)
            {
                return;
            }

            _taskService.MarkTaskAsCompleted(SelectedTask.Id);
            LoadTasks();
        }

        private void DeleteTask()
        {
            if (SelectedTask == null)
            {
                return;
            }

            _taskService.DeleteTask(SelectedTask.Id);
            LoadTasks();
        }

        private void RefreshExpiredTasks()
        {
            _taskService.UpdateExpiredTasks();
            LoadTasks();
            ActivityLogService.AddInfo("Выполнена проверка просроченных задач.");
        }
    }
}
