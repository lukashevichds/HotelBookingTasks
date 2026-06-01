using System;
using System.IO;
using System.Text.Json;
using Npgsql;

namespace HotelBookingTasks.Data
{
    public static class DbConnection
    {
        private const string FallbackConnectionString =
            "Host=localhost;Port=5432;Database=hotel_booking_tasks;Username=admin;Password=admin";

        private static readonly Lazy<string> LazyConnectionString = new(ReadConnectionString);

        public static string ConnectionString => LazyConnectionString.Value;

        public static NpgsqlConnection CreateOpenConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public static bool CanConnect(out string message)
        {
            try
            {
                using var connection = CreateOpenConnection();
                message = "Подключение активно";
                return true;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return false;
            }
        }

        private static string ReadConnectionString()
        {
            var currentDirectory = AppContext.BaseDirectory;

            while (!string.IsNullOrWhiteSpace(currentDirectory))
            {
                var appSettingsPath = Path.Combine(currentDirectory, "appsettings.json");
                if (File.Exists(appSettingsPath))
                {
                    try
                    {
                        using var stream = File.OpenRead(appSettingsPath);
                        using var document = JsonDocument.Parse(stream);

                        if (document.RootElement.TryGetProperty("ConnectionStrings", out var section) &&
                            section.TryGetProperty("DefaultConnection", out var value))
                        {
                            var connectionString = value.GetString();
                            if (!string.IsNullOrWhiteSpace(connectionString))
                            {
                                return connectionString;
                            }
                        }
                    }
                    catch
                    {
                        return FallbackConnectionString;
                    }
                }

                currentDirectory = Directory.GetParent(currentDirectory)?.FullName ?? string.Empty;
            }

            return FallbackConnectionString;
        }
    }
}
