using System.Windows.Controls;
using HotelBookingTasks.Models;
using HotelBookingTasks.ViewModels;

namespace HotelBookingTasks.Views
{
    public partial class BookingDetailsPage : Page
    {
        public BookingDetailsPage()
        {
            InitializeComponent();
            DataContext = new BookingDetailsViewModel();
        }

        public BookingDetailsPage(Booking booking) : this()
        {
            if (DataContext is BookingDetailsViewModel viewModel)
            {
                viewModel.SetBooking(booking);
            }
        }
    }
}
