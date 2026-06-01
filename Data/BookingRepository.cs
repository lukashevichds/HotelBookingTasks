using System;
using System.Collections.Generic;
using HotelBookingTasks.Models;
using Npgsql;

namespace HotelBookingTasks.Data
{
    public class BookingRepository
    {
        private const string BaseSelect = @"
            SELECT b.id, b.user_id, b.hotel_id, b.check_in_date, b.check_out_date,
                   b.booking_status, b.payment_status, b.total_price, b.created_at, b.notes,
                   COALESCE(u.full_name, '') AS user_full_name,
                   COALESCE(h.name, '') AS hotel_name,
                   COALESCE(h.city, '') AS hotel_city,
                   COALESCE(h.address, '') AS hotel_address,
                   COALESCE(h.stars, 0) AS hotel_stars,
                   COALESCE(h.requires_prepayment, FALSE) AS requires_prepayment
            FROM bookings b
            LEFT JOIN users u ON u.id = b.user_id
            LEFT JOIN hotels h ON h.id = b.hotel_id";

        public List<Booking> GetAll()
        {
            try
            {
                var bookings = new List<Booking>();

                using var connection = DbConnection.CreateOpenConnection();
                using var command = new NpgsqlCommand($"{BaseSelect} ORDER BY b.check_in_date ASC;", connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    bookings.Add(MapBooking(reader));
                }

                return bookings;
            }
            catch
            {
                return DemoDataStore.GetBookings();
            }
        }

        public List<Booking> GetByUserId(int userId)
        {
            try
            {
                var bookings = new List<Booking>();

                using var connection = DbConnection.CreateOpenConnection();
                using var command = new NpgsqlCommand($"{BaseSelect} WHERE b.user_id = @userId ORDER BY b.check_in_date ASC;", connection);
                command.Parameters.AddWithValue("@userId", userId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    bookings.Add(MapBooking(reader));
                }

                return bookings;
            }
            catch
            {
                return DemoDataStore.GetBookingsByUserId(userId);
            }
        }

        public Booking? GetById(int id)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                using var command = new NpgsqlCommand($"{BaseSelect} WHERE b.id = @id;", connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();
                return reader.Read() ? MapBooking(reader) : null;
            }
            catch
            {
                return DemoDataStore.GetBookingById(id);
            }
        }

        public void Add(Booking booking)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    INSERT INTO bookings
                    (user_id, hotel_id, check_in_date, check_out_date, booking_status,
                     payment_status, total_price, created_at, notes)
                    VALUES
                    (@userId, @hotelId, @checkInDate, @checkOutDate, @bookingStatus,
                     @paymentStatus, @totalPrice, @createdAt, @notes);";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", booking.UserId);
                command.Parameters.AddWithValue("@hotelId", booking.HotelId);
                command.Parameters.AddWithValue("@checkInDate", booking.CheckInDate);
                command.Parameters.AddWithValue("@checkOutDate", booking.CheckOutDate);
                command.Parameters.AddWithValue("@bookingStatus", booking.BookingStatus);
                command.Parameters.AddWithValue("@paymentStatus", booking.PaymentStatus);
                command.Parameters.AddWithValue("@totalPrice", booking.TotalPrice);
                command.Parameters.AddWithValue("@createdAt", booking.CreatedAt);
                command.Parameters.AddWithValue("@notes", booking.Notes ?? string.Empty);
                command.ExecuteNonQuery();
            }
            catch
            {
                DemoDataStore.AddBooking(booking);
            }
        }

        public void Update(Booking booking)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = @"
                    UPDATE bookings
                    SET user_id = @userId,
                        hotel_id = @hotelId,
                        check_in_date = @checkInDate,
                        check_out_date = @checkOutDate,
                        booking_status = @bookingStatus,
                        payment_status = @paymentStatus,
                        total_price = @totalPrice,
                        notes = @notes
                    WHERE id = @id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", booking.Id);
                command.Parameters.AddWithValue("@userId", booking.UserId);
                command.Parameters.AddWithValue("@hotelId", booking.HotelId);
                command.Parameters.AddWithValue("@checkInDate", booking.CheckInDate);
                command.Parameters.AddWithValue("@checkOutDate", booking.CheckOutDate);
                command.Parameters.AddWithValue("@bookingStatus", booking.BookingStatus);
                command.Parameters.AddWithValue("@paymentStatus", booking.PaymentStatus);
                command.Parameters.AddWithValue("@totalPrice", booking.TotalPrice);
                command.Parameters.AddWithValue("@notes", booking.Notes ?? string.Empty);
                command.ExecuteNonQuery();
            }
            catch
            {
                DemoDataStore.UpdateBooking(booking);
            }
        }

        public void Delete(int id)
        {
            try
            {
                using var connection = DbConnection.CreateOpenConnection();
                const string query = "DELETE FROM bookings WHERE id = @id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            catch
            {
                DemoDataStore.DeleteBooking(id);
            }
        }

        private static Booking MapBooking(NpgsqlDataReader reader)
        {
            return new Booking
            {
                Id = Convert.ToInt32(reader["id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                HotelId = Convert.ToInt32(reader["hotel_id"]),
                CheckInDate = Convert.ToDateTime(reader["check_in_date"]),
                CheckOutDate = Convert.ToDateTime(reader["check_out_date"]),
                BookingStatus = reader["booking_status"]?.ToString() ?? string.Empty,
                PaymentStatus = reader["payment_status"]?.ToString() ?? string.Empty,
                TotalPrice = Convert.ToDecimal(reader["total_price"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                Notes = reader["notes"]?.ToString() ?? string.Empty,
                UserFullName = reader["user_full_name"]?.ToString() ?? string.Empty,
                HotelName = reader["hotel_name"]?.ToString() ?? string.Empty,
                HotelCity = reader["hotel_city"]?.ToString() ?? string.Empty,
                HotelAddress = reader["hotel_address"]?.ToString() ?? string.Empty,
                HotelStars = Convert.ToInt32(reader["hotel_stars"]),
                RequiresPrepayment = Convert.ToBoolean(reader["requires_prepayment"])
            };
        }
    }
}
