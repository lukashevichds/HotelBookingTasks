using System;
using System.Collections.Generic;
using HotelBookingTasks.Models;
using Npgsql;

namespace HotelBookingTasks.Data
{
    public class UserRepository
    {
        public List<User> GetAll()
        {
            try
            {
                var users = new List<User>();

                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    SELECT id, full_name, email, phone, login, password, role
                    FROM users
                    ORDER BY id;";

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(MapUser(reader));
                }

                return users;
            }
            catch
            {
                return DemoDataStore.GetUsers();
            }
        }

        public User? GetById(int id)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    SELECT id, full_name, email, phone, login, password, role
                    FROM users
                    WHERE id = @id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }
            catch
            {
                return DemoDataStore.GetUserById(id);
            }
        }

        public User? GetByLogin(string login)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    SELECT id, full_name, email, phone, login, password, role
                    FROM users
                    WHERE login = @login;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", login);

                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }
            catch
            {
                return DemoDataStore.GetUserByLogin(login);
            }
        }

        public User? Authenticate(string login, string password)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    SELECT id, full_name, email, phone, login, password, role
                    FROM users
                    WHERE login = @login AND password = @password;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);

                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }
            catch
            {
                return DemoDataStore.Authenticate(login, password);
            }
        }

        public void Add(User user)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    INSERT INTO users (full_name, email, phone, login, password, role)
                    VALUES (@fullName, @email, @phone, @login, @password, @role);";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@fullName", user.FullName);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@phone", user.Phone);
                command.Parameters.AddWithValue("@login", user.Login);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@role", user.Role);
                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        public void Update(User user)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    UPDATE users
                    SET full_name = @fullName,
                        email = @email,
                        phone = @phone,
                        login = @login,
                        password = @password,
                        role = @role
                    WHERE id = @id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", user.Id);
                command.Parameters.AddWithValue("@fullName", user.FullName);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@phone", user.Phone);
                command.Parameters.AddWithValue("@login", user.Login);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@role", user.Role);
                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        public void Delete(int id)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = "DELETE FROM users WHERE id = @id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        private static User MapUser(NpgsqlDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["id"]),
                FullName = reader["full_name"]?.ToString() ?? string.Empty,
                Email = reader["email"]?.ToString() ?? string.Empty,
                Phone = reader["phone"]?.ToString() ?? string.Empty,
                Login = reader["login"]?.ToString() ?? string.Empty,
                Password = reader["password"]?.ToString() ?? string.Empty,
                Role = reader["role"]?.ToString() ?? string.Empty
            };
        }
    }
}
