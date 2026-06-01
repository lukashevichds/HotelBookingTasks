using System;
using System.Collections.Generic;
using HotelBookingTasks.Models;
using Npgsql;

namespace HotelBookingTasks.Data
{
    public class TaskRepository
    {
        private const string BaseSelect = @"
            SELECT t.id, t.booking_id, t.title, t.description, t.deadline,
                   t.priority, t.status, t.created_at, t.completed_at,
                   COALESCE(h.name, '') AS hotel_name,
                   COALESCE(h.city, '') AS hotel_city,
                   COALESCE(u.full_name, '') AS guest_name,
                   COALESCE(b.booking_status, '') AS booking_status,
                   b.check_in_date
            FROM tasks t
            INNER JOIN bookings b ON b.id = t.booking_id
            LEFT JOIN hotels h ON h.id = b.hotel_id
            LEFT JOIN users u ON u.id = b.user_id";

        public List<TaskItem> GetAll()
        {
            try
            {
                var tasks = new List<TaskItem>();

                using var connection = DbConnection.CreateOpenConnection();
                using var command = new NpgsqlCommand($"{BaseSelect} ORDER BY t.deadline ASC;", connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    tasks.Add(MapTask(reader));
                }

                return tasks;
            }
            catch
            {
                return DemoDataStore.GetTasks();
            }
        }

        public List<TaskItem> GetByBookingId(int bookingId)
        {
            try
            {
                var tasks = new List<TaskItem>();

                using var connection = DbConnection.CreateOpenConnection();
                using var command = new NpgsqlCommand($"{BaseSelect} WHERE t.booking_id = @bookingId ORDER BY t.deadline ASC;", connection);
                command.Parameters.AddWithValue("@bookingId", bookingId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(MapTask(reader));
                }

                return tasks;
            }
            catch
            {
                return DemoDataStore.GetTasksByBookingId(bookingId);
            }
        }

        public List<TaskItem> GetByUserId(int userId)
        {
            try
            {
                var tasks = new List<TaskItem>();

                using var connection = DbConnection.CreateOpenConnection();
                using var command = new NpgsqlCommand($"{BaseSelect} WHERE b.user_id = @userId ORDER BY t.deadline ASC;", connection);
                command.Parameters.AddWithValue("@userId", userId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(MapTask(reader));
                }

                return tasks;
            }
            catch
            {
                return DemoDataStore.GetTasksByUserId(userId);
            }
        }

        public TaskItem? GetById(int id)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                using var command = new NpgsqlCommand($"{BaseSelect} WHERE t.id = @id;", connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();
                return reader.Read() ? MapTask(reader) : null;
            }
            catch
            {
                return DemoDataStore.GetTaskById(id);
            }
        }

        public void Add(TaskItem task)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    INSERT INTO tasks
                    (booking_id, title, description, deadline, priority, status, created_at, completed_at)
                    VALUES
                    (@bookingId, @title, @description, @deadline, @priority, @status, @createdAt, @completedAt);";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@bookingId", task.BookingId);
                command.Parameters.AddWithValue("@title", task.Title);
                command.Parameters.AddWithValue("@description", task.Description ?? string.Empty);
                command.Parameters.AddWithValue("@deadline", task.Deadline);
                command.Parameters.AddWithValue("@priority", task.Priority);
                command.Parameters.AddWithValue("@status", task.Status);
                command.Parameters.AddWithValue("@createdAt", task.CreatedAt);
                object completedAt = task.CompletedAt.HasValue
                    ? task.CompletedAt.Value
                    : DBNull.Value;
                command.Parameters.AddWithValue("@completedAt", completedAt);
                command.ExecuteNonQuery();
            }
            catch
            {
                DemoDataStore.AddTask(task);
            }
        }

        public void Update(TaskItem task)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    UPDATE tasks
                    SET booking_id = @bookingId,
                        title = @title,
                        description = @description,
                        deadline = @deadline,
                        priority = @priority,
                        status = @status,
                        completed_at = @completedAt
                    WHERE id = @id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", task.Id);
                command.Parameters.AddWithValue("@bookingId", task.BookingId);
                command.Parameters.AddWithValue("@title", task.Title);
                command.Parameters.AddWithValue("@description", task.Description ?? string.Empty);
                command.Parameters.AddWithValue("@deadline", task.Deadline);
                command.Parameters.AddWithValue("@priority", task.Priority);
                command.Parameters.AddWithValue("@status", task.Status);
                object completedAt = task.CompletedAt.HasValue
                    ? task.CompletedAt.Value
                    : DBNull.Value;
                command.Parameters.AddWithValue("@completedAt", completedAt);
                command.ExecuteNonQuery();
            }
            catch
            {
                DemoDataStore.UpdateTask(task);
            }
        }

        public void Delete(int id)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = "DELETE FROM tasks WHERE id = @id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            catch
            {
                DemoDataStore.DeleteTask(id);
            }
        }

        public void MarkAsCompleted(int id)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    UPDATE tasks
                    SET status = @status,
                        completed_at = @completedAt
                    WHERE id = @id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@status", TaskItemStatus.Completed);
                command.Parameters.AddWithValue("@completedAt", DateTime.Now);
                command.ExecuteNonQuery();
            }
            catch
            {
                DemoDataStore.MarkTaskCompleted(id);
            }
        }

        private static TaskItem MapTask(NpgsqlDataReader reader)
        {
            return new TaskItem
            {
                Id = Convert.ToInt32(reader["id"]),
                BookingId = Convert.ToInt32(reader["booking_id"]),
                Title = reader["title"]?.ToString() ?? string.Empty,
                Description = reader["description"]?.ToString() ?? string.Empty,
                Deadline = Convert.ToDateTime(reader["deadline"]),
                Priority = reader["priority"]?.ToString() ?? string.Empty,
                Status = reader["status"]?.ToString() ?? string.Empty,
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                CompletedAt = reader["completed_at"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["completed_at"]),
                HotelName = reader["hotel_name"]?.ToString() ?? string.Empty,
                HotelCity = reader["hotel_city"]?.ToString() ?? string.Empty,
                GuestName = reader["guest_name"]?.ToString() ?? string.Empty,
                BookingStatus = reader["booking_status"]?.ToString() ?? string.Empty,
                BookingCheckInDate = reader["check_in_date"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["check_in_date"])
            };
        }
    }
}
