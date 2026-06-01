using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Input;
using HotelBookingTasks.ViewModels;

namespace HotelBookingTasks.Views
{
    public partial class ChatBotsPage : Page
    {
        public ChatBotsPage()
        {
            InitializeComponent();

            var viewModel = new ChatBotsViewModel();
            DataContext = viewModel;
            viewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        }

        private ChatBotsViewModel ViewModel => (ChatBotsViewModel)DataContext;

        private void CommandTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || !ViewModel.SendCommand.CanExecute(null))
            {
                return;
            }

            ViewModel.SendCommand.Execute(null);
            e.Handled = true;
        }

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (MessagesListBox.Items.Count > 0)
                {
                    MessagesListBox.ScrollIntoView(MessagesListBox.Items[^1]);
                }
            });
        }
    }
}
