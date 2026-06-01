using HotelBookingTasks.Data;
using HotelBookingTasks.Models;

namespace HotelBookingTasks.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        public static User? CurrentUser { get; private set; }

        public AuthService()
        {
            _userRepository = new UserRepository();
        }

        public User? Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ActivityLogService.AddWarning("Попытка входа с пустым логином или паролем.");
                return null;
            }

            var user = _userRepository.Authenticate(login.Trim(), password.Trim());
            CurrentUser = user;

            if (user == null)
            {
                ActivityLogService.AddWarning($"Неуспешная авторизация для логина '{login.Trim()}'.");
            }
            else
            {
                ActivityLogService.AddInfo($"Выполнен вход пользователя '{user.FullName}'.");
            }

            return user;
        }

        public static void Logout()
        {
            if (CurrentUser != null)
            {
                ActivityLogService.AddInfo($"Пользователь '{CurrentUser.FullName}' вышел из приложения.");
            }

            CurrentUser = null;
        }
    }
}
