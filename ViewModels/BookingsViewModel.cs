using System.Collections.ObjectModel;
using System.Linq;
using HotelBookingTasks.Models;
using HotelBookingTasks.Services;
using HotelBookingTasks.Helpers;

namespace HotelBookingTasks.ViewModels
{
    public class BookingsViewModel : BaseViewModel
    {
        private readonly BookingService _bookingService;
        private readonly TaskGenerationService _taskGenerationService;

        public ObservableCollection<Booking> Bookings { get; }

        private Booking? _selectedBooking;
        public Booking? SelectedBooking
        {
            get => _selectedBooking;
            set
            {
                if (SetProperty(ref _selectedBooking, value))
                {
                    GenerateTasksCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private int _totalBookings;
        public int TotalBookings
        {
            get => _totalBookings;
            set => SetProperty(ref _totalBookings, value);
        }

        private int _activeBookings;
        public int ActiveBookings
        {
            get => _activeBookings;
            set => SetProperty(ref _activeBookings, value);
        }

        private int _completedBookings;
        public int CompletedBookings
        {
            get => _completedBookings;
            set => SetProperty(ref _completedBookings, value);
        }

        public RelayCommand LoadBookingsCommand { get; }
        public RelayCommand GenerateTasksCommand { get; }

        public BookingsViewModel()
        {
            _bookingService = new BookingService();
            _taskGenerationService = new TaskGenerationService();
            Bookings = new ObservableCollection<Booking>();

            LoadBookingsCommand = new RelayCommand(_ => LoadBookings());
            GenerateTasksCommand = new RelayCommand(_ => GenerateTasks(), _ => SelectedBooking != null);

            LoadBookings();
        }

        public void LoadBookings()
        {
            Bookings.Clear();

            var bookings = _bookingService.GetBookingsForCurrentUser();
            foreach (var booking in bookings)
            {
                Bookings.Add(booking);
            }

            TotalBookings = Bookings.Count;
            ActiveBookings = Bookings.Count(item => item.IsActive);
            CompletedBookings = Bookings.Count(item => item.IsCompleted);
        }

        private void GenerateTasks()
        {
            if (SelectedBooking == null)
            {
                return;
            }

            _taskGenerationService.GenerateTasksForBooking(SelectedBooking.Id);
            ActivityLogService.AddInfo($"Запрошена генерация задач для бронирования #{SelectedBooking.Id}.");
        }
    }
}
