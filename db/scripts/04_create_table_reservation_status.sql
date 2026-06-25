-- =============================================================================
-- Script: 04_create_table_reservation_status.sql
-- Type:   Master table (no foreign keys)
-- Source: ER entity "StatusReservations"
-- =============================================================================

CREATE TABLE IF NOT EXISTS public.reservation_status
(
    id          uuid         NOT NULL DEFAULT gen_random_uuid(),
    status_name varchar(100) NOT NULL,
    description text         NULL,
    CONSTRAINT pk_reservation_status PRIMARY KEY (id)
);
