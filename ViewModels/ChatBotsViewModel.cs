using System.Collections.ObjectModel;
using System.Linq;
using HotelBookingTasks.Helpers;
using HotelBookingTasks.Models;
using HotelBookingTasks.Services;

namespace HotelBookingTasks.ViewModels
{
    public class ChatBotsViewModel : BaseViewModel
    {
        private readonly ChatBotService _chatBotService;

        private ChatBotScenario? _selectedBotScenario;
        private string _userInput = string.Empty;

        public ObservableCollection<ChatBotScenario> BotScenarios { get; }
        public ObservableCollection<ChatCommandOption> QuickCommands { get; }
        public ObservableCollection<ChatMessage> Messages { get; }

        public ChatBotScenario? SelectedBotScenario
        {
            get => _selectedBotScenario;
            set
            {
                if (SetProperty(ref _selectedBotScenario, value))
                {
                    LoadQuickCommands();
                    StartScenarioDialog();
                    SendCommand.RaiseCanExecuteChanged();
                    RunQuickCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string UserInput
        {
            get => _userInput;
            set
            {
                if (SetProperty(ref _userInput, value))
                {
                    SendCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand SendCommand { get; }
        public RelayCommand RunQuickCommand { get; }
        public RelayCommand ClearChatCommand { get; }

        public ChatBotsViewModel()
        {
            _chatBotService = new ChatBotService();

            BotScenarios = new ObservableCollection<ChatBotScenario>();
            QuickCommands = new ObservableCollection<ChatCommandOption>();
            Messages = new ObservableCollection<ChatMessage>();

            SendCommand = new RelayCommand(_ => SendUserCommand(UserInput, true), _ => CanSendUserInput());
            RunQuickCommand = new RelayCommand(SendQuickCommand, _ => SelectedBotScenario != null);
            ClearChatCommand = new RelayCommand(_ => StartScenarioDialog());

            foreach (var scenario in _chatBotService.GetScenarios())
            {
                BotScenarios.Add(scenario);
            }

            SelectedBotScenario = BotScenarios.FirstOrDefault();
        }

        private bool CanSendUserInput()
        {
            return SelectedBotScenario != null && !string.IsNullOrWhiteSpace(UserInput);
        }

        private void SendQuickCommand(object? parameter)
        {
            if (parameter is not string command)
            {
                return;
            }

            SendUserCommand(command, false);
        }

        private void SendUserCommand(string command, bool clearInput)
        {
            if (SelectedBotScenario == null || string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            var normalizedCommand = command.Trim();

            Messages.Add(new ChatMessage
            {
                SenderName = "Пользователь",
                Text = normalizedCommand,
                IsUserMessage = true
            });

            var botResponse = _chatBotService.ProcessCommand(SelectedBotScenario.Id, normalizedCommand);

            Messages.Add(new ChatMessage
            {
                SenderName = SelectedBotScenario.Name,
                Text = botResponse,
                IsUserMessage = false
            });

            ActivityLogService.AddInfo($"Чат-бот '{SelectedBotScenario.Name}' обработал команду '{normalizedCommand}'.");

            if (clearInput)
            {
                UserInput = string.Empty;
            }
        }

        private void LoadQuickCommands()
        {
            QuickCommands.Clear();

            if (SelectedBotScenario == null)
            {
                return;
            }

            foreach (var command in _chatBotService.GetQuickCommands(SelectedBotScenario.Id))
            {
                QuickCommands.Add(command);
            }
        }

        private void StartScenarioDialog()
        {
            Messages.Clear();

            if (SelectedBotScenario == null)
            {
                return;
            }

            Messages.Add(new ChatMessage
            {
                SenderName = SelectedBotScenario.Name,
                Text = _chatBotService.GetWelcomeMessage(SelectedBotScenario.Id),
                IsUserMessage = false
            });
        }
    }
}
