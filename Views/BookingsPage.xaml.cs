using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HotelBookingTasks.ViewModels;

namespace HotelBookingTasks.Views
{
    public partial class BookingsPage : Page
    {
        public BookingsPage()
        {
            InitializeComponent();
            DataContext = new BookingsViewModel();
        }

        private BookingsViewModel ViewModel => (BookingsViewModel)DataContext;

        private void OpenDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedBooking();
        }

        private void BookingsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedBooking();
        }

        private void OpenSelectedBooking()
        {
            if (ViewModel.SelectedBooking == null)
            {
                return;
            }

            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToBookingDetails(ViewModel.SelectedBooking);
            }
        }
    }
}
