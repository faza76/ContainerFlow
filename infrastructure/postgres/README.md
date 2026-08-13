# PostgreSQL Infrastructure

## Database-per-Service Decision

This project uses three physically separate PostgreSQL containers:

- `booking-db` → `booking_db`
- `container-db` → `container_db`
- `notification-db` → `notification_db`

This was chosen instead of a single PostgreSQL instance containing three databases because
the architecture intentionally follows the database-per-service pattern.

Each backend service owns its database:

- Booking API can access only `booking-db`.
- Container API can access only `container-db`.
- Notification API can access only `notification-db`.

A service should never directly query another service's database.

This makes the ownership boundary physically visible and provides stronger service
independence. It also makes the architecture easier to evolve later, because each service
can potentially migrate to a different database technology without forcing the other
services to change.

## Why Not One PostgreSQL Container?

A single PostgreSQL instance with three databases would also provide logical isolation,
but all services would still share the same database infrastructure.

For this project, physical separation is preferred because the project demonstrates
microservice architecture and service independence.

## Initialization

There are currently no shared PostgreSQL initialization scripts.

Each PostgreSQL container initializes its own database using the following environment
variables from `.env`:

- `BOOKING_DB_NAME`
- `BOOKING_DB_USER`
- `BOOKING_DB_PASSWORD`
- `CONTAINER_DB_NAME`
- `CONTAINER_DB_USER`
- `CONTAINER_DB_PASSWORD`
- `NOTIFICATION_DB_NAME`
- `NOTIFICATION_DB_USER`
- `NOTIFICATION_DB_PASSWORD`

Schema creation and migrations remain the responsibility of the corresponding service.
