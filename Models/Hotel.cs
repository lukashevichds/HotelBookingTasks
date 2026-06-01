namespace HotelBookingTasks.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Stars { get; set; }
        public bool RequiresPrepayment { get; set; }
        public string Description { get; set; } = string.Empty;

        public string DisplayName => $"{Name}, {City}";
    }
}
