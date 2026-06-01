using System.Collections.Generic;
using HotelBookingTasks.Data;
using HotelBookingTasks.Models;

namespace HotelBookingTasks.Services
{
    public class BookingService
    {
        private readonly BookingRepository _bookingRepository;

        public BookingService()
        {
            _bookingRepository = new BookingRepository();
        }

        public List<Booking> GetAllBookings()
        {
            return _bookingRepository.GetAll();
        }

        public List<Booking> GetBookingsByUserId(int userId)
        {
            return _bookingRepository.GetByUserId(userId);
        }

        public List<Booking> GetBookingsForCurrentUser()
        {
            var currentUser = AuthService.CurrentUser;
            if (currentUser == null)
            {
                return new List<Booking>();
            }

            return currentUser.Role == "Admin"
                ? GetAllBookings()
                : GetBookingsByUserId(currentUser.Id);
        }

        public Booking? GetBookingById(int id)
        {
            return _bookingRepository.GetById(id);
        }

        public void AddBooking(Booking booking)
        {
            _bookingRepository.Add(booking);
        }

        public void UpdateBooking(Booking booking)
        {
            _bookingRepository.Update(booking);
        }

        public void DeleteBooking(int id)
        {
            _bookingRepository.Delete(id);
        }
    }
}
