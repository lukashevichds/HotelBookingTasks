using System;

namespace HotelBookingTasks.Models
{
    public class ChatMessage
    {
        public string SenderName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsUserMessage { get; set; }
    }
}
