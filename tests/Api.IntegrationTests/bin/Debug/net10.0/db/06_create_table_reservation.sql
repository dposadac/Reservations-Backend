-- =============================================================================
-- Script: 06_create_table_reservation.sql
-- Type:   Dependent table (foreign keys to master/dependent tables)
-- Source: ER entity "Reservations" + RF-03/04/05
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
    reservation_code      varchar(20)  NULL,           -- RF-04: código único EV-######
    cancelled_at          timestamptz  NULL,           -- RF-05: fecha/hora de cancelación
    is_forfeited          boolean      NOT NULL DEFAULT false, -- RN-07: entradas perdidas
    CONSTRAINT pk_reservation PRIMARY KEY (id),
    CONSTRAINT fk_reservation_event
        FOREIGN KEY (event_id)              REFERENCES public.event (event_id),
    CONSTRAINT fk_reservation_reservation_status
        FOREIGN KEY (reservation_status_id) REFERENCES public.reservation_status (id),
    CONSTRAINT ck_reservation_quantity CHECK (quantity > 0),
    CONSTRAINT ck_reservation_code_format
        CHECK (reservation_code IS NULL OR reservation_code ~ '^EV-[0-9]{6}$')
);

CREATE INDEX IF NOT EXISTS ix_reservation_event_id  ON public.reservation (event_id);
CREATE INDEX IF NOT EXISTS ix_reservation_status_id ON public.reservation (reservation_status_id);

-- RF-04: el código de reserva es único cuando está asignado (las reservas sin confirmar son NULL).
CREATE UNIQUE INDEX IF NOT EXISTS ux_reservation_code
    ON public.reservation (reservation_code)
    WHERE reservation_code IS NOT NULL;
