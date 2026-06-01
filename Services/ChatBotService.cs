using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HotelBookingTasks.Models;

namespace HotelBookingTasks.Services
{
    public class ChatBotService
    {
        private const string BookingScenarioId = "booking";
        private const string SupportScenarioId = "support";

        private readonly BookingService _bookingService;
        private readonly TaskService _taskService;

        public ChatBotService()
        {
            _bookingService = new BookingService();
            _taskService = new TaskService();
        }

        public List<ChatBotScenario> GetScenarios()
        {
            return new List<ChatBotScenario>
            {
                new()
                {
                    Id = BookingScenarioId,
                    Name = "Бот бронирований",
                    Description = "Проверка статуса брони, оплаты и задач по номеру бронирования."
                },
                new()
                {
                    Id = SupportScenarioId,
                    Name = "Бот поддержки",
                    Description = "Информационные ответы по заселению, отмене, контактам и трансферу."
                }
            };
        }

        public List<ChatCommandOption> GetQuickCommands(string scenarioId)
        {
            if (scenarioId == BookingScenarioId)
            {
                var options = new List<ChatCommandOption>
                {
                    CreateOption("Помощь", "/help", "Показать команды бота бронирований"),
                    CreateOption("Мои брони", "/bookings", "Показать доступные бронирования")
                };

                var exampleBookingId = GetExampleBookingId();
                if (exampleBookingId.HasValue)
                {
                    options.Add(CreateOption($"Бронь #{exampleBookingId}", $"/booking {exampleBookingId}", "Показать карточку бронирования"));
                    options.Add(CreateOption($"Оплата #{exampleBookingId}", $"/payment {exampleBookingId}", "Показать статус оплаты"));
                    options.Add(CreateOption($"Задачи #{exampleBookingId}", $"/tasks {exampleBookingId}", "Показать задачи по бронированию"));
                }

                return options;
            }

            if (scenarioId == SupportScenarioId)
            {
                var options = new List<ChatCommandOption>
                {
                    CreateOption("Помощь", "/help", "Показать команды бота поддержки"),
                    CreateOption("Контакты", "/contacts", "Показать контакты поддержки"),
                    CreateOption("Заселение", "/checkin", "Показать правила заселения"),
                    CreateOption("Отмена", "/cancel", "Показать правила отмены")
                };

                var transferBookingId = GetTransferBookingExampleId();
                if (transferBookingId.HasValue)
                {
                    options.Add(CreateOption($"Трансфер #{transferBookingId}", $"/transfer {transferBookingId}", "Проверить запрос на трансфер"));
                }

                options.Add(CreateOption("Оператор", "/operator", "Передать вопрос оператору"));
                return options;
            }

            return new List<ChatCommandOption>();
        }

        public string GetWelcomeMessage(string scenarioId)
        {
            return scenarioId switch
            {
                BookingScenarioId =>
                    "Я бот бронирований. Я понимаю только команды: /help, /bookings, /booking <id>, /payment <id>, /tasks <id>.",
                SupportScenarioId =>
                    "Я бот поддержки. Я понимаю только команды: /help, /contacts, /checkin, /cancel, /transfer <id>, /operator.",
                _ => "Выберите сценарий чат-бота."
            };
        }

        public string ProcessCommand(string scenarioId, string command)
        {
            var normalizedCommand = command.Trim();
            if (string.IsNullOrWhiteSpace(normalizedCommand))
            {
                return "Введите команду. Для списка команд используйте /help.";
            }

            var parts = normalizedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var verb = parts[0].ToLowerInvariant();

            return scenarioId switch
            {
                BookingScenarioId => ProcessBookingCommand(verb, parts),
                SupportScenarioId => ProcessSupportCommand(verb, parts),
                _ => "Сценарий чат-бота не найден."
            };
        }

        private string ProcessBookingCommand(string verb, string[] parts)
        {
            return verb switch
            {
                "/help" => BuildBookingHelp(),
                "/bookings" => BuildBookingsList(),
                "/booking" => BuildBookingDetails(parts),
                "/payment" => BuildPaymentDetails(parts),
                "/tasks" => BuildBookingTasks(parts),
                _ => BuildUnknownCommandMessage()
            };
        }

        private string ProcessSupportCommand(string verb, string[] parts)
        {
            return verb switch
            {
                "/help" => BuildSupportHelp(),
                "/contacts" => "Поддержка доступна ежедневно с 08:00 до 22:00. Телефон: +7 (999) 000-00-03. Email: support@hotel-booking.local.",
                "/checkin" => "Стандартное заселение начинается с 14:00, выезд до 12:00. Для раннего заезда добавьте пожелание в примечание к бронированию.",
                "/cancel" => "Отмена обрабатывается оператором. Бесплатная отмена возможна, если она разрешена правилами отеля и выполнена до даты заезда.",
                "/transfer" => BuildTransferDetails(parts),
                "/operator" => BuildOperatorRequest(),
                _ => BuildUnknownCommandMessage()
            };
        }

        private int? GetExampleBookingId()
        {
            return _bookingService
                .GetBookingsForCurrentUser()
                .OrderByDescending(item => item.IsActive)
                .ThenBy(item => item.CheckInDate)
                .Select(item => (int?)item.Id)
                .FirstOrDefault();
        }

        private int? GetTransferBookingExampleId()
        {
            var bookings = _bookingService
                .GetBookingsForCurrentUser()
                .OrderByDescending(item => item.IsActive)
                .ThenBy(item => item.CheckInDate)
                .ToList();

            return bookings.FirstOrDefault(item => item.NeedsTransfer)?.Id ??
                   bookings.Select(item => (int?)item.Id).FirstOrDefault();
        }

        private string BuildBookingsList()
        {
            var bookings = _bookingService.GetBookingsForCurrentUser();
            if (bookings.Count == 0)
            {
                return "Доступных бронирований нет.";
            }

            return "Доступные бронирования:" + Environment.NewLine +
                   string.Join(
                       Environment.NewLine,
                       bookings.Select(item =>
                           $"#{item.Id}: {item.HotelDisplay}, {item.StayPeriod}, статус: {item.BookingStatus}, оплата: {item.PaymentStatus}."));
        }

        private string BuildBookingDetails(string[] parts)
        {
            if (!TryGetBooking(parts, out var booking, out var errorMessage))
            {
                return errorMessage;
            }

            return $"Бронирование #{booking.Id}" + Environment.NewLine +
                   $"Отель: {booking.HotelDisplay}" + Environment.NewLine +
                   $"Адрес: {booking.HotelAddress}" + Environment.NewLine +
                   $"Клиент: {booking.UserFullName}" + Environment.NewLine +
                   $"Период: {booking.StayPeriod}" + Environment.NewLine +
                   $"Статус брони: {booking.BookingStatus}" + Environment.NewLine +
                   $"Примечание: {GetBookingNotes(booking)}";
        }

        private string BuildPaymentDetails(string[] parts)
        {
            if (!TryGetBooking(parts, out var booking, out var errorMessage))
            {
                return errorMessage;
            }

            var prepaymentText = booking.RequiresPrepayment
                ? "Для этого отеля требуется предоплата."
                : "Предоплата для этого отеля не обязательна.";

            return $"Оплата по бронированию #{booking.Id}: {booking.PaymentStatus}." + Environment.NewLine +
                   $"Стоимость: {booking.TotalPrice.ToString("N0", CultureInfo.GetCultureInfo("ru-RU"))} руб." + Environment.NewLine +
                   prepaymentText;
        }

        private string BuildBookingTasks(string[] parts)
        {
            if (!TryGetBooking(parts, out var booking, out var errorMessage))
            {
                return errorMessage;
            }

            _taskService.UpdateExpiredTasks();
            var tasks = _taskService.GetTasksByBookingId(booking.Id);
            if (tasks.Count == 0)
            {
                return $"По бронированию #{booking.Id} задач пока нет.";
            }

            return $"Задачи по бронированию #{booking.Id}:" + Environment.NewLine +
                   string.Join(
                       Environment.NewLine,
                       tasks.Select(item =>
                           $"- {item.Title}: {item.EffectiveStatus}, срок {item.Deadline:dd.MM.yyyy HH:mm}."));
        }

        private string BuildTransferDetails(string[] parts)
        {
            if (!TryGetBooking(parts, out var booking, out var errorMessage))
            {
                return errorMessage;
            }

            if (booking.NeedsTransfer)
            {
                return $"По бронированию #{booking.Id} уже указан запрос на трансфер. Оператор должен подтвердить детали до заезда.";
            }

            return $"По бронированию #{booking.Id} запрос на трансфер не указан. Для добавления трансфера обратитесь к оператору.";
        }

        private bool TryGetBooking(string[] parts, out Booking booking, out string errorMessage)
        {
            booking = new Booking();
            errorMessage = string.Empty;

            if (parts.Length < 2 || !int.TryParse(parts[1], out var bookingId))
            {
                errorMessage = "Укажите номер бронирования. Пример: /booking 1.";
                return false;
            }

            var foundBooking = _bookingService
                .GetBookingsForCurrentUser()
                .FirstOrDefault(item => item.Id == bookingId);

            if (foundBooking == null)
            {
                errorMessage = $"Бронирование #{bookingId} не найдено или недоступно текущему пользователю.";
                return false;
            }

            booking = foundBooking;
            return true;
        }

        private static ChatCommandOption CreateOption(string title, string command, string description)
        {
            return new ChatCommandOption
            {
                Title = title,
                Command = command,
                Description = description
            };
        }

        private static string BuildBookingHelp()
        {
            return "Команды бота бронирований:" + Environment.NewLine +
                   "/bookings - список доступных бронирований" + Environment.NewLine +
                   "/booking <id> - подробности бронирования" + Environment.NewLine +
                   "/payment <id> - статус оплаты" + Environment.NewLine +
                   "/tasks <id> - задачи по бронированию";
        }

        private static string BuildSupportHelp()
        {
            return "Команды бота поддержки:" + Environment.NewLine +
                   "/contacts - контакты службы поддержки" + Environment.NewLine +
                   "/checkin - правила заселения и выезда" + Environment.NewLine +
                   "/cancel - правила отмены" + Environment.NewLine +
                   "/transfer <id> - статус запроса на трансфер" + Environment.NewLine +
                   "/operator - передать вопрос оператору";
        }

        private static string BuildOperatorRequest()
        {
            ActivityLogService.AddWarning("Клиент запросил оператора через чат-бота поддержки.");
            return "Запрос передан оператору. Обращение зафиксировано в журнале действий.";
        }

        private static string BuildUnknownCommandMessage()
        {
            return "Команда не распознана. Этот бот работает только по заданным командам. Используйте /help.";
        }

        private static string GetBookingNotes(Booking booking)
        {
            return string.IsNullOrWhiteSpace(booking.Notes)
                ? "не указано"
                : booking.Notes;
        }
    }
}
