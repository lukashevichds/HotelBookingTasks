using System.Windows.Controls;
using HotelBookingTasks.ViewModels;

namespace HotelBookingTasks.Views
{
    public partial class TasksPage : Page
    {
        public TasksPage()
        {
            InitializeComponent();
            DataContext = new TasksViewModel();
        }
    }
}
