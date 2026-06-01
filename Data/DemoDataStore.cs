using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingTasks.Models;

namespace HotelBookingTasks.Data
{
    internal static class DemoDataStore
    {
        private static readonly object SyncRoot = new();
        private static bool _initialized;
        private static List<User> _users = new();
        private static List<Hotel> _hotels = new();
        private static List<Booking> _bookings = new();
        private static List<TaskItem> _tasks = new();
        private static int _nextBookingId = 1;
        private static int _nextTaskId = 1;

        public static List<User> GetUsers()
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                return _users.Select(CloneUser).ToList();
            }
        }

        public static User? GetUserById(int id)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var user = _users.FirstOrDefault(item => item.Id == id);
                return user == null ? null : CloneUser(user);
            }
        }

        public static User? GetUserByLogin(string login)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var user = _users.FirstOrDefault(item =>
                    string.Equals(item.Login, login, StringComparison.OrdinalIgnoreCase));

                return user == null ? null : CloneUser(user);
            }
        }

        public static User? Authenticate(string login, string password)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var user = _users.FirstOrDefault(item =>
                    string.Equals(item.Login, login, StringComparison.OrdinalIgnoreCase) &&
                    item.Password == password);

                return user == null ? null : CloneUser(user);
            }
        }

        public static List<Booking> GetBookings()
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                return _bookings
                    .OrderBy(item => item.CheckInDate)
                    .Select(ComposeBooking)
                    .ToList();
            }
        }

        public static List<Booking> GetBookingsByUserId(int userId)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                return _bookings
                    .Where(item => item.UserId == userId)
                    .OrderBy(item => item.CheckInDate)
                    .Select(ComposeBooking)
                    .ToList();
            }
        }

        public static Booking? GetBookingById(int id)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var booking = _bookings.FirstOrDefault(item => item.Id == id);
                return booking == null ? null : ComposeBooking(booking);
            }
        }

        public static void AddBooking(Booking booking)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var clone = CloneBooking(booking);
                clone.Id = _nextBookingId++;
                clone.CreatedAt = booking.CreatedAt == default ? DateTime.Now : booking.CreatedAt;
                _bookings.Add(clone);
            }
        }

        public static void UpdateBooking(Booking booking)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var existing = _bookings.FirstOrDefault(item => item.Id == booking.Id);
                if (existing == null)
                {
                    return;
                }

                existing.UserId = booking.UserId;
                existing.HotelId = booking.HotelId;
                existing.CheckInDate = booking.CheckInDate;
                existing.CheckOutDate = booking.CheckOutDate;
                existing.BookingStatus = booking.BookingStatus;
                existing.PaymentStatus = booking.PaymentStatus;
                existing.TotalPrice = booking.TotalPrice;
                existing.Notes = booking.Notes;
            }
        }

        public static void DeleteBooking(int id)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                _bookings.RemoveAll(item => item.Id == id);
                _tasks.RemoveAll(item => item.BookingId == id);
            }
        }

        public static List<TaskItem> GetTasks()
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                return _tasks
                    .OrderBy(item => item.Deadline)
                    .Select(ComposeTask)
                    .ToList();
            }
        }

        public static List<TaskItem> GetTasksByBookingId(int bookingId)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                return _tasks
                    .Where(item => item.BookingId == bookingId)
                    .OrderBy(item => item.Deadline)
                    .Select(ComposeTask)
                    .ToList();
            }
        }

        public static List<TaskItem> GetTasksByUserId(int userId)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var bookingIds = _bookings
                    .Where(item => item.UserId == userId)
                    .Select(item => item.Id)
                    .ToHashSet();

                return _tasks
                    .Where(item => bookingIds.Contains(item.BookingId))
                    .OrderBy(item => item.Deadline)
                    .Select(ComposeTask)
                    .ToList();
            }
        }

        public static TaskItem? GetTaskById(int id)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var task = _tasks.FirstOrDefault(item => item.Id == id);
                return task == null ? null : ComposeTask(task);
            }
        }

        public static void AddTask(TaskItem task)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                if (_tasks.Any(item =>
                    item.BookingId == task.BookingId &&
                    string.Equals(item.Title, task.Title, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                var clone = CloneTask(task);
                clone.Id = _nextTaskId++;
                clone.CreatedAt = task.CreatedAt == default ? DateTime.Now : task.CreatedAt;
                _tasks.Add(clone);
            }
        }

        public static void UpdateTask(TaskItem task)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var existing = _tasks.FirstOrDefault(item => item.Id == task.Id);
                if (existing == null)
                {
                    return;
                }

                existing.BookingId = task.BookingId;
                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.Deadline = task.Deadline;
                existing.Priority = task.Priority;
                existing.Status = task.Status;
                existing.CompletedAt = task.CompletedAt;
            }
        }

        public static void DeleteTask(int id)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                _tasks.RemoveAll(item => item.Id == id);
            }
        }

        public static void MarkTaskCompleted(int id)
        {
            EnsureInitialized();
            lock (SyncRoot)
            {
                var task = _tasks.FirstOrDefault(item => item.Id == id);
                if (task == null)
                {
                    return;
                }

                task.Status = TaskItemStatus.Completed;
                task.CompletedAt = DateTime.Now;
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_initialized)
                {
                    return;
                }

                Seed();
                _initialized = true;
            }
        }

        private static void Seed()
        {
            var today = DateTime.Today;

            _users = new List<User>
            {
                new()
                {
                    Id = 1,
                    FullName = "Иван Петров",
                    Email = "ivan.petrov@example.com",
                    Phone = "+79990000001",
                    Login = "ivan",
                    Password = "1234",
                    Role = "Client"
                },
                new()
                {
                    Id = 2,
                    FullName = "Анна Смирнова",
                    Email = "anna.smirnova@example.com",
                    Phone = "+79990000002",
                    Login = "anna",
                    Password = "1234",
                    Role = "Client"
                },
                new()
                {
                    Id = 3,
                    FullName = "Администратор",
                    Email = "admin@example.com",
                    Phone = "+79990000003",
                    Login = "admin",
                    Password = "admin",
                    Role = "Admin"
                }
            };

            _hotels = new List<Hotel>
            {
                new()
                {
                    Id = 1,
                    Name = "Grand Palace Hotel",
                    City = "Москва",
                    Address = "ул. Тверская, 10",
                    Stars = 5,
                    RequiresPrepayment = true,
                    Description = "Пятизвездочный отель в центре города"
                },
                new()
                {
                    Id = 2,
                    Name = "Sea Breeze Resort",
                    City = "Сочи",
                    Address = "ул. Морская, 25",
                    Stars = 4,
                    RequiresPrepayment = true,
                    Description = "Курортный отель рядом с морем"
                },
                new()
                {
                    Id = 3,
                    Name = "City Comfort Inn",
                    City = "Санкт-Петербург",
                    Address = "Невский проспект, 50",
                    Stars = 3,
                    RequiresPrepayment = false,
                    Description = "Уютный городской отель для деловых поездок"
                }
            };

            _bookings = new List<Booking>
            {
                new()
                {
                    Id = 1,
                    UserId = 1,
                    HotelId = 1,
                    CheckInDate = today.AddDays(5).AddHours(14),
                    CheckOutDate = today.AddDays(9).AddHours(12),
                    BookingStatus = "Подтверждено",
                    PaymentStatus = "Частично оплачено",
                    TotalPrice = 45200m,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    Notes = "Нужен ранний заезд"
                },
                new()
                {
                    Id = 2,
                    UserId = 2,
                    HotelId = 2,
                    CheckInDate = today.AddDays(2).AddHours(14),
                    CheckOutDate = today.AddDays(7).AddHours(12),
                    BookingStatus = "Создано",
                    PaymentStatus = "Не оплачено",
                    TotalPrice = 72800m,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Notes = "Запрос на трансфер из аэропорта"
                },
                new()
                {
                    Id = 3,
                    UserId = 1,
                    HotelId = 3,
                    CheckInDate = today.AddDays(-7).AddHours(14),
                    CheckOutDate = today.AddDays(-4).AddHours(12),
                    BookingStatus = "Завершено",
                    PaymentStatus = "Оплачено",
                    TotalPrice = 12300m,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    Notes = "Командировка"
                }
            };

            _tasks = new List<TaskItem>
            {
                new()
                {
                    Id = 1,
                    BookingId = 1,
                    Title = "Проверить данные клиента",
                    Description = "Проверить паспортные и контактные данные клиента.",
                    Deadline = DateTime.Now.AddHours(6),
                    Priority = "Высокий",
                    Status = TaskItemStatus.InProgress,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new()
                {
                    Id = 2,
                    BookingId = 1,
                    Title = "Оплатить остаток",
                    Description = "Внести оставшуюся сумму по бронированию до даты заезда.",
                    Deadline = DateTime.Now.AddDays(-1),
                    Priority = "Высокий",
                    Status = TaskItemStatus.Overdue,
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new()
                {
                    Id = 3,
                    BookingId = 2,
                    Title = "Подготовить документы",
                    Description = "Подготовить подтверждение бронирования и документы для заселения.",
                    Deadline = DateTime.Now.AddDays(1),
                    Priority = "Средний",
                    Status = TaskItemStatus.New,
                    CreatedAt = DateTime.Now.AddHours(-12)
                },
                new()
                {
                    Id = 4,
                    BookingId = 3,
                    Title = "Оставить отзыв",
                    Description = "Предложить клиенту оставить отзыв после проживания.",
                    Deadline = DateTime.Now.AddDays(-3),
                    Priority = "Низкий",
                    Status = TaskItemStatus.Completed,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    CompletedAt = DateTime.Now.AddDays(-3).AddHours(2)
                }
            };

            _nextBookingId = _bookings.Max(item => item.Id) + 1;
            _nextTaskId = _tasks.Max(item => item.Id) + 1;
        }

        private static Booking ComposeBooking(Booking booking)
        {
            var clone = CloneBooking(booking);
            var hotel = _hotels.FirstOrDefault(item => item.Id == booking.HotelId);
            var user = _users.FirstOrDefault(item => item.Id == booking.UserId);

            clone.HotelName = hotel?.Name ?? string.Empty;
            clone.HotelCity = hotel?.City ?? string.Empty;
            clone.HotelAddress = hotel?.Address ?? string.Empty;
            clone.HotelStars = hotel?.Stars ?? 0;
            clone.RequiresPrepayment = hotel?.RequiresPrepayment ?? false;
            clone.UserFullName = user?.FullName ?? string.Empty;

            return clone;
        }

        private static TaskItem ComposeTask(TaskItem task)
        {
            var clone = CloneTask(task);
            var booking = _bookings.FirstOrDefault(item => item.Id == task.BookingId);

            if (booking == null)
            {
                return clone;
            }

            var hotel = _hotels.FirstOrDefault(item => item.Id == booking.HotelId);
            var user = _users.FirstOrDefault(item => item.Id == booking.UserId);

            clone.HotelName = hotel?.Name ?? string.Empty;
            clone.HotelCity = hotel?.City ?? string.Empty;
            clone.GuestName = user?.FullName ?? string.Empty;
            clone.BookingStatus = booking.BookingStatus;
            clone.BookingCheckInDate = booking.CheckInDate;

            return clone;
        }

        private static User CloneUser(User user)
        {
            return new User
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Login = user.Login,
                Password = user.Password,
                Role = user.Role
            };
        }

        private static Booking CloneBooking(Booking booking)
        {
            return new Booking
            {
                Id = booking.Id,
                UserId = booking.UserId,
                HotelId = booking.HotelId,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                BookingStatus = booking.BookingStatus,
                PaymentStatus = booking.PaymentStatus,
                TotalPrice = booking.TotalPrice,
                CreatedAt = booking.CreatedAt,
                Notes = booking.Notes,
                UserFullName = booking.UserFullName,
                HotelName = booking.HotelName,
                HotelCity = booking.HotelCity,
                HotelAddress = booking.HotelAddress,
                HotelStars = booking.HotelStars,
                RequiresPrepayment = booking.RequiresPrepayment
            };
        }

        private static TaskItem CloneTask(TaskItem task)
        {
            return new TaskItem
            {
                Id = task.Id,
                BookingId = task.BookingId,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Priority = task.Priority,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt,
                HotelName = task.HotelName,
                HotelCity = task.HotelCity,
                GuestName = task.GuestName,
                BookingStatus = task.BookingStatus,
                BookingCheckInDate = task.BookingCheckInDate
            };
        }
    }
}
