DROP TABLE IF EXISTS tasks CASCADE;
DROP TABLE IF EXISTS bookings CASCADE;
DROP TABLE IF EXISTS hotels CASCADE;
DROP TABLE IF EXISTS users CASCADE;

CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    full_name VARCHAR(200) NOT NULL,
    email VARCHAR(200) NOT NULL,
    phone VARCHAR(50),
    login VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(100) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'Client'
);

CREATE TABLE hotels (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    city VARCHAR(100) NOT NULL,
    address VARCHAR(300) NOT NULL,
    stars INT NOT NULL CHECK (stars >= 1 AND stars <= 5),
    requires_prepayment BOOLEAN NOT NULL DEFAULT FALSE,
    description TEXT
);

CREATE TABLE bookings (
    id SERIAL PRIMARY KEY,
    user_id INT NOT NULL,
    hotel_id INT NOT NULL,
    check_in_date TIMESTAMP NOT NULL,
    check_out_date TIMESTAMP NOT NULL,
    booking_status VARCHAR(50) NOT NULL,
    payment_status VARCHAR(50) NOT NULL,
    total_price NUMERIC(10, 2) NOT NULL CHECK (total_price >= 0),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    notes TEXT,
    CONSTRAINT fk_bookings_user
        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_bookings_hotel
        FOREIGN KEY (hotel_id) REFERENCES hotels(id) ON DELETE CASCADE,
    CONSTRAINT chk_booking_dates
        CHECK (check_out_date > check_in_date),
    CONSTRAINT chk_booking_status
        CHECK (booking_status IN ('Создано', 'Подтверждено', 'Завершено', 'Отменено')),
    CONSTRAINT chk_payment_status
        CHECK (payment_status IN ('Не оплачено', 'Частично оплачено', 'Оплачено'))
);

CREATE TABLE tasks (
    id SERIAL PRIMARY KEY,
    booking_id INT NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    deadline TIMESTAMP NOT NULL,
    priority VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,
    CONSTRAINT uq_tasks_booking_title UNIQUE (booking_id, title),
    CONSTRAINT fk_tasks_booking
        FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE CASCADE,
    CONSTRAINT chk_task_priority
        CHECK (priority IN ('Низкий', 'Средний', 'Высокий')),
    CONSTRAINT chk_task_status
        CHECK (status IN ('Новая', 'В работе', 'Выполнено', 'Просрочено', 'Отменено'))
);

CREATE INDEX idx_bookings_user_id ON bookings(user_id);
CREATE INDEX idx_bookings_hotel_id ON bookings(hotel_id);
CREATE INDEX idx_tasks_booking_id ON tasks(booking_id);
CREATE INDEX idx_tasks_deadline ON tasks(deadline);
