using System.Collections.ObjectModel;
using System.Linq;
using HotelBookingTasks.Helpers;
using HotelBookingTasks.Models;
using HotelBookingTasks.Services;

namespace HotelBookingTasks.ViewModels
{
    public class BookingDetailsViewModel : BaseViewModel
    {
        private readonly TaskService _taskService;
        private readonly TaskGenerationService _taskGenerationService;

        private Booking? _selectedBooking;
        public Booking? SelectedBooking
        {
            get => _selectedBooking;
            set
            {
                if (SetProperty(ref _selectedBooking, value))
                {
                    OnPropertyChanged(nameof(BookingTitle));
                    OnPropertyChanged(nameof(BookingSubtitle));
                    OnPropertyChanged(nameof(NotesText));
                    GenerateTasksCommand.RaiseCanExecuteChanged();
                    RefreshTasksCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<TaskItem> Tasks { get; }

        private TaskItem? _selectedTask;
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }

        private int _totalTasks;
        public int TotalTasks
        {
            get => _totalTasks;
            set => SetProperty(ref _totalTasks, value);
        }

        private int _overdueTasks;
        public int OverdueTasks
        {
            get => _overdueTasks;
            set => SetProperty(ref _overdueTasks, value);
        }

        public string BookingTitle =>
            SelectedBooking == null
                ? "Карточка бронирования"
                : $"Бронирование №{SelectedBooking.Id} • {SelectedBooking.HotelDisplay}";

        public string BookingSubtitle =>
            SelectedBooking == null
                ? "Выберите бронирование из списка."
                : $"{SelectedBooking.UserFullName} • {SelectedBooking.StayPeriod}";

        public string NotesText =>
            string.IsNullOrWhiteSpace(SelectedBooking?.Notes)
                ? "Дополнительные пожелания не указаны."
                : SelectedBooking!.Notes;

        public RelayCommand GenerateTasksCommand { get; }
        public RelayCommand RefreshTasksCommand { get; }
        public RelayCommand MarkTaskCompletedCommand { get; }

        public BookingDetailsViewModel()
        {
            _taskService = new TaskService();
            _taskGenerationService = new TaskGenerationService();
            Tasks = new ObservableCollection<TaskItem>();

            GenerateTasksCommand = new RelayCommand(_ => GenerateTasks(), _ => SelectedBooking != null);
            RefreshTasksCommand = new RelayCommand(_ => LoadTasks(), _ => SelectedBooking != null);
            MarkTaskCompletedCommand = new RelayCommand(MarkTaskCompleted);
        }

        public void SetBooking(Booking booking)
        {
            SelectedBooking = booking;
            _taskGenerationService.GenerateTasksForBooking(booking.Id);
            LoadTasks();
        }

        public void LoadTasks()
        {
            Tasks.Clear();

            if (SelectedBooking == null)
            {
                return;
            }

            _taskService.UpdateExpiredTasks();

            foreach (var task in _taskService.GetTasksByBookingId(SelectedBooking.Id))
            {
                Tasks.Add(task);
            }

            TotalTasks = Tasks.Count;
            OverdueTasks = Tasks.Count(item => item.IsOverdue);
        }

        private void GenerateTasks()
        {
            if (SelectedBooking == null)
            {
                return;
            }

            _taskGenerationService.GenerateTasksForBooking(SelectedBooking.Id);
            LoadTasks();
        }

        private void MarkTaskCompleted(object? parameter)
        {
            if (parameter is not TaskItem task || task.IsCompleted)
            {
                return;
            }

            _taskService.MarkTaskAsCompleted(task.Id);
            LoadTasks();
        }
    }
}
