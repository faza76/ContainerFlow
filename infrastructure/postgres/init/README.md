# PostgreSQL Init

## Decision: separate containers (option 1)

ContainerFlow uses **three physically separate PostgreSQL containers** — one per
service — rather than one container with three databases:

| Container service | Hostname | Database |
|---|---|---|
| `booking-db` | `booking-db:5432` | `booking_db` |
| `container-db` | `container-db:5432` | `container_db` |
| `notification-db` | `notification-db:5432` | `notification_db` |

**Why:** the README's database-per-service pattern is a core portfolio talking point.
Physical separation makes the isolation boundary real (a compromised or buggy service
*cannot* reach another service's data at the network level, not just by convention),
which is far more convincing in an interview than three databases behind one instance.

**Trade-off accepted:** slightly higher memory footprint vs. a single Postgres; the
isolation win is worth it for this project.

## Init scripts

`init/` is intentionally empty. Each database is initialized entirely by its
Postgres container's `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD`
environment variables (sourced from `.env` via docker-compose.yml):

- `BOOKING_DB_NAME` / `BOOKING_DB_USER` / `BOOKING_DB_PASSWORD`
- `CONTAINER_DB_NAME` / `CONTAINER_DB_USER` / `CONTAINER_DB_PASSWORD`
- `NOTIFICATION_DB_NAME` / `NOTIFICATION_DB_USER` / `NOTIFICATION_DB_PASSWORD`

Schema creation and migrations are the responsibility of each service (EF Core
migrations applied at startup by the owning service).

If a future change requires shared SQL setup, drop `.sql` files here and mount them
into the relevant Postgres container's `/docker-entrypoint-initdb.d/`.
