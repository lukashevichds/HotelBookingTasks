using System;
using System.Collections.Generic;
using HotelBookingTasks.Data;
using HotelBookingTasks.Models;

namespace HotelBookingTasks.Services
{
    public class TaskService
    {
        private readonly TaskRepository _taskRepository;

        public TaskService()
        {
            _taskRepository = new TaskRepository();
        }

        public List<TaskItem> GetAllTasks()
        {
            return _taskRepository.GetAll();
        }

        public List<TaskItem> GetTasksByBookingId(int bookingId)
        {
            return _taskRepository.GetByBookingId(bookingId);
        }

        public List<TaskItem> GetTasksForCurrentUser()
        {
            var currentUser = AuthService.CurrentUser;
            if (currentUser == null)
            {
                return new List<TaskItem>();
            }

            return currentUser.Role == "Admin"
                ? GetAllTasks()
                : _taskRepository.GetByUserId(currentUser.Id);
        }

        public TaskItem? GetTaskById(int id)
        {
            return _taskRepository.GetById(id);
        }

        public void AddTask(TaskItem task)
        {
            _taskRepository.Add(task);
        }

        public void UpdateTask(TaskItem task)
        {
            _taskRepository.Update(task);
        }

        public void DeleteTask(int id)
        {
            var task = _taskRepository.GetById(id);
            _taskRepository.Delete(id);

            if (task != null)
            {
                ActivityLogService.AddWarning($"Удалена задача '{task.Title}' по бронированию #{task.BookingId}.");
            }
        }

        public void MarkTaskAsCompleted(int id)
        {
            var task = _taskRepository.GetById(id);
            _taskRepository.MarkAsCompleted(id);

            if (task != null)
            {
                ActivityLogService.AddInfo($"Задача '{task.Title}' отмечена как выполненная.");
            }
        }

        public void UpdateExpiredTasks()
        {
            var tasks = _taskRepository.GetAll();

            foreach (var task in tasks)
            {
                if (task.Status != TaskItemStatus.Completed &&
                    task.Status != TaskItemStatus.Cancelled &&
                    task.Deadline < DateTime.Now)
                {
                    task.Status = TaskItemStatus.Overdue;
                    _taskRepository.Update(task);
                }
            }
        }
    }
}
