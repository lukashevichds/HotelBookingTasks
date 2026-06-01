using System.Windows.Controls;
using HotelBookingTasks.ViewModels;

namespace HotelBookingTasks.Views
{
    public partial class MonitoringPage : Page
    {
        public MonitoringPage()
        {
            InitializeComponent();
            DataContext = new MonitoringViewModel();
        }
    }
}
