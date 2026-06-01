TRUNCATE TABLE tasks, bookings, hotels, users RESTART IDENTITY CASCADE;

INSERT INTO users (full_name, email, phone, login, password, role) VALUES
('Иван Петров', 'ivan.petrov@example.com', '+79990000001', 'ivan', '1234', 'Client'),
('Анна Смирнова', 'anna.smirnova@example.com', '+79990000002', 'anna', '1234', 'Client'),
('Администратор', 'admin@example.com', '+79990000003', 'admin', 'admin', 'Admin');

INSERT INTO hotels (name, city, address, stars, requires_prepayment, description) VALUES
('Grand Palace Hotel', 'Москва', 'ул. Тверская, 10', 5, TRUE, 'Пятизвездочный отель в центре города'),
('Sea Breeze Resort', 'Сочи', 'ул. Морская, 25', 4, TRUE, 'Курортный отель рядом с морем'),
('City Comfort Inn', 'Санкт-Петербург', 'Невский проспект, 50', 3, FALSE, 'Уютный городской отель для деловых поездок');

INSERT INTO bookings (user_id, hotel_id, check_in_date, check_out_date, booking_status, payment_status, total_price, created_at, notes) VALUES
(1, 1, CURRENT_DATE + INTERVAL '5 day' + INTERVAL '14 hour', CURRENT_DATE + INTERVAL '9 day' + INTERVAL '12 hour', 'Подтверждено', 'Частично оплачено', 45200.00, CURRENT_TIMESTAMP - INTERVAL '2 day', 'Нужен ранний заезд'),
(2, 2, CURRENT_DATE + INTERVAL '2 day' + INTERVAL '14 hour', CURRENT_DATE + INTERVAL '7 day' + INTERVAL '12 hour', 'Создано', 'Не оплачено', 72800.00, CURRENT_TIMESTAMP - INTERVAL '1 day', 'Запрос на трансфер из аэропорта'),
(1, 3, CURRENT_DATE - INTERVAL '7 day' + INTERVAL '14 hour', CURRENT_DATE - INTERVAL '4 day' + INTERVAL '12 hour', 'Завершено', 'Оплачено', 12300.00, CURRENT_TIMESTAMP - INTERVAL '10 day', 'Командировка');

INSERT INTO tasks (booking_id, title, description, deadline, priority, status, created_at, completed_at) VALUES
(1, 'Проверить данные клиента', 'Проверить корректность паспортных и контактных данных.', CURRENT_TIMESTAMP + INTERVAL '6 hour', 'Высокий', 'В работе', CURRENT_TIMESTAMP - INTERVAL '1 day', NULL),
(1, 'Оплатить остаток', 'Внести оставшуюся сумму по бронированию.', CURRENT_TIMESTAMP - INTERVAL '1 day', 'Высокий', 'Просрочено', CURRENT_TIMESTAMP - INTERVAL '2 day', NULL),
(2, 'Подготовить документы', 'Подготовить документы и подтверждение для заселения.', CURRENT_TIMESTAMP + INTERVAL '1 day', 'Средний', 'Новая', CURRENT_TIMESTAMP - INTERVAL '12 hour', NULL),
(3, 'Оставить отзыв', 'Предложить клиенту оставить отзыв после завершения проживания.', CURRENT_TIMESTAMP - INTERVAL '3 day', 'Низкий', 'Выполнено', CURRENT_TIMESTAMP - INTERVAL '5 day', CURRENT_TIMESTAMP - INTERVAL '3 day' + INTERVAL '2 hour');
