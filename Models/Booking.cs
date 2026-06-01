using System;

namespace HotelBookingTasks.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public string HotelCity { get; set; } = string.Empty;
        public string HotelAddress { get; set; } = string.Empty;
        public int HotelStars { get; set; }
        public bool RequiresPrepayment { get; set; }

        public int NightsCount => Math.Max(1, (CheckOutDate.Date - CheckInDate.Date).Days);

        public bool IsActive => BookingStatus == "Создано" || BookingStatus == "Подтверждено";

        public bool IsCompleted => BookingStatus == "Завершено";

        public bool NeedsTransfer =>
            Notes.Contains("трансфер", StringComparison.OrdinalIgnoreCase);

        public string HotelDisplay =>
            string.IsNullOrWhiteSpace(HotelCity) ? HotelName : $"{HotelName}, {HotelCity}";

        public string StayPeriod =>
            $"{CheckInDate:dd.MM.yyyy HH:mm} - {CheckOutDate:dd.MM.yyyy HH:mm}";
    }
}
