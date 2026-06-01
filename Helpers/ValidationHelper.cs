using System.Text.RegularExpressions;

namespace HotelBookingTasks.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsLoginValid(string login)
        {
            return !string.IsNullOrWhiteSpace(login) && login.Length >= 3;
        }

        public static bool IsPasswordValid(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 4;
        }

        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase
            );
        }

        public static bool IsPhoneValid(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return Regex.IsMatch(phone, @"^\+?[0-9]{10,15}$");
        }

        public static bool IsBookingDatesValid(System.DateTime checkInDate, System.DateTime checkOutDate)
        {
            return checkOutDate > checkInDate;
        }

        public static bool IsRequiredTextFilled(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsPriceValid(decimal price)
        {
            return price >= 0;
        }
    }
}