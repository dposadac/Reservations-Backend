-- =============================================================================
-- Script: 06_create_table_reservation.sql
-- Type:   Dependent table (foreign keys to master/dependent tables)
-- Source: ER entity "Reservations"
-- Depends on: event (05), reservation_status (04)
-- =============================================================================

CREATE TABLE IF NOT EXISTS public.reservation
(
    id                    uuid         NOT NULL DEFAULT gen_random_uuid(),
    event_id              uuid         NOT NULL,
    reservation_status_id uuid         NOT NULL,
    quantity              integer      NOT NULL DEFAULT 1,
    purchaser_name        varchar(50)  NOT NULL,
    purchaser_email       varchar(150) NOT NULL,
    city                  varchar(50)  NULL,
    CONSTRAINT pk_reservation PRIMARY KEY (id),
    CONSTRAINT fk_reservation_event
        FOREIGN KEY (event_id)              REFERENCES public.event (event_id),
    CONSTRAINT fk_reservation_reservation_status
        FOREIGN KEY (reservation_status_id) REFERENCES public.reservation_status (id),
    CONSTRAINT ck_reservation_quantity CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_reservation_event_id  ON public.reservation (event_id);
CREATE INDEX IF NOT EXISTS ix_reservation_status_id ON public.reservation (reservation_status_id);
