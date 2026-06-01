using System.Linq;
using System.Windows;
using HotelBookingTasks.Helpers;
using HotelBookingTasks.Models;
using HotelBookingTasks.Services;

namespace HotelBookingTasks.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public RelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(_ => ExecuteLogin());
        }

        private void ExecuteLogin()
        {
            ErrorMessage = string.Empty;

            if (!ValidationHelper.IsLoginValid(Login))
            {
                ErrorMessage = "Введите корректный логин";
                return;
            }

            if (!ValidationHelper.IsPasswordValid(Password))
            {
                ErrorMessage = "Введите корректный пароль";
                return;
            }

            CurrentUser = _authService.Login(Login, Password);

            if (CurrentUser == null)
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();

            Application.Current.Windows
                .OfType<Views.LoginWindow>()
                .FirstOrDefault()
                ?.Close();
        }
    }
}
