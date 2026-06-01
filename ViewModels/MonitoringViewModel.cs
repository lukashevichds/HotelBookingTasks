using System;
using System.Collections.ObjectModel;
using System.Linq;
using HotelBookingTasks.Data;
using HotelBookingTasks.Helpers;
using HotelBookingTasks.Models;
using HotelBookingTasks.Services;

namespace HotelBookingTasks.ViewModels
{
    public class MonitoringViewModel : BaseViewModel
    {
        private readonly TaskService _taskService;
        private readonly BookingService _bookingService;

        private string _databaseStatus = "Не проверено";
        public string DatabaseStatus
        {
            get => _databaseStatus;
            set => SetProperty(ref _databaseStatus, value);
        }

        private string _dataMode = "Не определен";
        public string DataMode
        {
            get => _dataMode;
            set => SetProperty(ref _dataMode, value);
        }

        private int _totalBookings;
        public int TotalBookings
        {
            get => _totalBookings;
            set => SetProperty(ref _totalBookings, value);
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

        private DateTime _lastUpdated;
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        public ObservableCollection<TaskItem> OverdueTaskList { get; }
        public ObservableCollection<string> RecentActions { get; }

        public RelayCommand RefreshMonitoringCommand { get; }

        public MonitoringViewModel()
        {
            _taskService = new TaskService();
            _bookingService = new BookingService();

            OverdueTaskList = new ObservableCollection<TaskItem>();
            RecentActions = ActivityLogService.Entries;
            RefreshMonitoringCommand = new RelayCommand(_ => LoadMonitoringData());

            LoadMonitoringData();
        }

        public void LoadMonitoringData()
        {
            CheckDatabaseConnection();
            _taskService.UpdateExpiredTasks();

            var bookings = _bookingService.GetAllBookings();
            var tasks = _taskService.GetAllTasks();

            TotalBookings = bookings.Count;
            TotalTasks = tasks.Count;
            ActiveTasks = tasks.Count(task => !task.IsCompleted && task.Status != TaskItemStatus.Cancelled);

            OverdueTaskList.Clear();
            var overdueTasks = tasks
                .Where(task => task.IsOverdue)
                .OrderBy(task => task.Deadline)
                .ToList();

            OverdueTasks = overdueTasks.Count;
            foreach (var task in overdueTasks)
            {
                OverdueTaskList.Add(task);
            }

            LastUpdated = DateTime.Now;
        }

        private void CheckDatabaseConnection()
        {
            var hasConnection = DbConnection.CanConnect(out _);
            DatabaseStatus = hasConnection ? "Подключение активно" : "Подключение недоступно";
            DataMode = hasConnection ? "PostgreSQL" : "Демо-режим";
        }
    }
}
