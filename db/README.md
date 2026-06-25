# Database scripts (Database First)

This project uses **PostgreSQL** with a **Database First** approach: the schema is
defined and versioned through the SQL scripts in `db/scripts`. **No EF Core
migrations are generated.**

The model is derived from the ER diagram [`Diagrama-ER-Events.jpg`](Diagrama-ER-Events.jpg).
The `ApplicationDbContext` / `TodoItemConfiguration` only *map* the entities to the
tables these scripts create, and must be kept in sync manually.

## Data model (English)

### Master tables (no foreign keys)

| Table                 | Columns                                                        |
|-----------------------|---------------------------------------------------------------|
| `event_status`        | `id` PK, `status_name`, `description`                         |
| `venue`               | `id` PK, `venue_name`, `quantity`, `city`                    |
| `event_type`          | `id` PK, `event_type_name`                                    |
| `reservation_status`  | `id` PK, `status_name`, `description`                        |

### Dependent tables (with foreign keys)

| Table         | Columns                                                                                                   | Foreign keys |
|---------------|-----------------------------------------------------------------------------------------------------------|--------------|
| `event`       | `event_id` PK, `description`, `venue_id`, `maximum_capacity`, `start_date`, `end_date`, `ticket_price`, `event_type_id`, `status_event_id` | `venue_id` → `venue.id`, `event_type_id` → `event_type.id`, `status_event_id` → `event_status.id` |
| `reservation` | `id` PK, `event_id`, `reservation_status_id`, `quantity`, `purchaser_name`, `purchaser_email`, `city`     | `event_id` → `event.event_id`, `reservation_status_id` → `reservation_status.id` |

### Relationships

```
event_status (1) ──< event           (one status, many events)
venue        (1) ──< event           (one venue, many events)
event_type   (1) ──< event           (one type, many events)
event        (1) ──< reservation     (one event, many reservations)
reservation_status (1) ──< reservation (one status, many reservations)
```

> Mapping note: the ER `Event` entity lists a second `eventId: uuid FK`. It models the
> relationship to `EventType` and is implemented as the `event_type_id` column.

## Execution order

Scripts are applied in ascending numeric order. The database is created first, then
the master tables, then the dependent tables.

| #  | Script                                   | Type             |
|----|------------------------------------------|------------------|
| 00 | `00_create_database.sql`                 | Database         |
| 01 | `01_create_table_event_status.sql`       | Master           |
| 02 | `02_create_table_venue.sql`              | Master           |
| 03 | `03_create_table_event_type.sql`         | Master           |
| 04 | `04_create_table_reservation_status.sql` | Master           |
| 05 | `05_create_table_event.sql`              | Dependent (FK)   |
| 06 | `06_create_table_reservation.sql`        | Dependent (FK)   |
| 07 | `07_create_table_todo_items.sql`         | Sample / baseline |
| 08 | `08_seed_todo_items.sql`                 | Sample data      |

## How to apply the scripts

```powershell
$env:PGPASSWORD = "postgres"

# 1. Create the database (connect to the 'postgres' maintenance database)
psql -h localhost -U postgres -d postgres -f db/scripts/00_create_database.sql

# 2. Create tables (connect to the new database, in order)
psql -h localhost -U postgres -d ceiba_reservations -f db/scripts/01_create_table_event_status.sql
psql -h localhost -U postgres -d ceiba_reservations -f db/scripts/02_create_table_venue.sql
psql -h localhost -U postgres -d ceiba_reservations -f db/scripts/03_create_table_event_type.sql
psql -h localhost -U postgres -d ceiba_reservations -f db/scripts/04_create_table_reservation_status.sql
psql -h localhost -U postgres -d ceiba_reservations -f db/scripts/05_create_table_event.sql
psql -h localhost -U postgres -d ceiba_reservations -f db/scripts/06_create_table_reservation.sql
psql -h localhost -U postgres -d ceiba_reservations -f db/scripts/07_create_table_todo_items.sql
psql -h localhost -U postgres -d ceiba_reservations -f db/scripts/08_seed_todo_items.sql
```

Or apply every script in order with a single loop:

```powershell
$env:PGPASSWORD = "postgres"
Get-ChildItem db/scripts/*.sql | Where-Object Name -ne '00_create_database.sql' |
    Sort-Object Name |
    ForEach-Object { psql -h localhost -U postgres -d ceiba_reservations -f $_.FullName }
```

The integration tests (`tests/Api.IntegrationTests`) apply
`07_create_table_todo_items.sql` automatically against an ephemeral PostgreSQL
container (Testcontainers), demonstrating the Database First flow end to end.
