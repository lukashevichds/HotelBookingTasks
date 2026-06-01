using System;
using System.Collections.Generic;
using HotelBookingTasks.Data;
using HotelBookingTasks.Models;

namespace HotelBookingTasks.Services
{
    public class TaskGenerationService
    {
        private readonly TaskRepository _taskRepository;
        private readonly BookingRepository _bookingRepository;

        public TaskGenerationService()
        {
            _taskRepository = new TaskRepository();
            _bookingRepository = new BookingRepository();
        }

        public void GenerateTasksForBooking(int bookingId)
        {
            var booking = _bookingRepository.GetById(bookingId);
            if (booking == null)
                return;

            var existingTasks = _taskRepository.GetByBookingId(bookingId);

            AddTaskIfNotExists(
                existingTasks,
                bookingId,
                "Проверить данные клиента",
                "Проверить корректность контактных данных и параметров бронирования",
                booking.CreatedAt.AddHours(2),
                "Высокий"
            );

            if (booking.RequiresPrepayment && booking.PaymentStatus != "Оплачено")
            {
                AddTaskIfNotExists(
                    existingTasks,
                    bookingId,
                    "Оплатить остаток",
                    "Необходимо внести оплату по бронированию до даты заезда",
                    booking.CheckInDate.AddDays(-2),
                    "Высокий"
                );
            }

            if ((booking.CheckInDate - DateTime.Now).TotalDays <= 3 || booking.BookingStatus == "Подтверждено")
            {
                AddTaskIfNotExists(
                    existingTasks,
                    bookingId,
                    "Подготовить документы",
                    "Подготовить документы и информацию для заселения.",
                    booking.CheckInDate.AddDays(-1),
                    "Средний"
                );
            }

            if (booking.NeedsTransfer)
            {
                AddTaskIfNotExists(
                    existingTasks,
                    bookingId,
                    "Заказать трансфер",
                    "Проверить и подтвердить организацию трансфера для клиента.",
                    booking.CheckInDate.AddDays(-1),
                    "Средний"
                );
            }

            if (booking.BookingStatus == "Завершено" || booking.CheckOutDate < DateTime.Now)
            {
                AddTaskIfNotExists(
                    existingTasks,
                    bookingId,
                    "Оставить отзыв",
                    "Предложить клиенту оставить отзыв после завершения проживания.",
                    booking.CheckOutDate.AddDays(1),
                    "Низкий"
                );
            }

            ActivityLogService.AddInfo($"Проверены правила автогенерации задач для бронирования #{bookingId}.");
        }

        private void AddTaskIfNotExists(
            List<TaskItem> existingTasks,
            int bookingId,
            string title,
            string description,
            DateTime deadline,
            string priority)
        {
            foreach (var task in existingTasks)
            {
                if (task.Title == title)
                    return;
            }

            var newTask = new TaskItem
            {
                BookingId = bookingId,
                Title = title,
                Description = description,
                Deadline = deadline,
                Priority = priority,
                Status = TaskItemStatus.New,
                CreatedAt = DateTime.Now
            };

            _taskRepository.Add(newTask);
            existingTasks.Add(newTask);
        }
    }
}
