using System;
using System.Collections.ObjectModel;

namespace HotelBookingTasks.Services
{
    public static class ActivityLogService
    {
        public static ObservableCollection<string> Entries { get; } = new();

        public static void AddInfo(string message)
        {
            Add("INFO", message);
        }

        public static void AddWarning(string message)
        {
            Add("WARN", message);
        }

        public static void AddError(string message)
        {
            Add("ERROR", message);
        }

        private static void Add(string level, string message)
        {
            var entry = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} | {level} | {message}";
            Entries.Insert(0, entry);

            while (Entries.Count > 50)
            {
                Entries.RemoveAt(Entries.Count - 1);
            }
        }
    }
}
