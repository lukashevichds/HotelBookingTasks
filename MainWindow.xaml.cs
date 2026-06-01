using System.Windows;
using HotelBookingTasks.Models;
using HotelBookingTasks.Services;
using HotelBookingTasks.Views;

namespace HotelBookingTasks
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ConfigureShell();
            NavigateToBookings();
        }

        private void BookingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToBookings();
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TasksPage());
        }

        private void ChatBotsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ChatBotsPage());
        }

        private void MonitoringButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MonitoringPage());
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        public void NavigateToBookingDetails(Booking booking)
        {
            MainFrame.Navigate(new BookingDetailsPage(booking));
        }

        private void NavigateToBookings()
        {
            MainFrame.Navigate(new BookingsPage());
        }

        private void ConfigureShell()
        {
            var user = AuthService.CurrentUser;

            CurrentUserTextBlock.Text = user?.FullName ?? "Неизвестный пользователь";
            CurrentRoleTextBlock.Text = $"Роль: {user?.Role ?? "Не определена"}";
            MonitoringButton.Visibility = user?.Role == "Admin"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
