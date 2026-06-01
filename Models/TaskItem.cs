using System;

namespace HotelBookingTasks.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelCity { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime? BookingCheckInDate { get; set; }

        public bool IsCompleted => Status == TaskItemStatus.Completed;

        public bool IsOverdue =>
            !IsCompleted &&
            Status != TaskItemStatus.Cancelled &&
            Deadline < DateTime.Now;

        public string EffectiveStatus =>
            IsOverdue && Status != TaskItemStatus.Overdue ? TaskItemStatus.Overdue : Status;

        public string BookingDisplay =>
            string.IsNullOrWhiteSpace(HotelCity)
                ? HotelName
                : $"{HotelName}, {HotelCity}";
    }
}
