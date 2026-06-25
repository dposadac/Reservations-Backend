-- =============================================================================
-- Script: 05_create_table_event.sql
-- Type:   Dependent table (foreign keys to master tables)
-- Source: ER entity "Event"
-- Depends on: venue (02), event_type (03), event_status (01)
--
-- NOTE: In the ER diagram the second "eventId: uuid FK" of Event models the
--       relationship to EventType; it is mapped here as event_type_id.
-- =============================================================================

CREATE TABLE IF NOT EXISTS public.event
(
    event_id         uuid          NOT NULL DEFAULT gen_random_uuid(),
    description      text          NULL,
    venue_id         uuid          NOT NULL,
    maximum_capacity integer       NOT NULL DEFAULT 0,
    start_date       timestamptz   NOT NULL,
    end_date         timestamptz   NOT NULL,
    ticket_price     numeric(18, 2) NOT NULL DEFAULT 0,
    event_type_id    uuid          NOT NULL,
    status_event_id  uuid          NOT NULL,
    CONSTRAINT pk_event PRIMARY KEY (event_id),
    CONSTRAINT fk_event_venue
        FOREIGN KEY (venue_id)        REFERENCES public.venue (id),
    CONSTRAINT fk_event_event_type
        FOREIGN KEY (event_type_id)   REFERENCES public.event_type (id),
    CONSTRAINT fk_event_event_status
        FOREIGN KEY (status_event_id) REFERENCES public.event_status (id),
    CONSTRAINT ck_event_capacity CHECK (maximum_capacity >= 0),
    CONSTRAINT ck_event_ticket_price CHECK (ticket_price >= 0),
    CONSTRAINT ck_event_dates CHECK (end_date >= start_date)
);

CREATE INDEX IF NOT EXISTS ix_event_venue_id        ON public.event (venue_id);
CREATE INDEX IF NOT EXISTS ix_event_event_type_id   ON public.event (event_type_id);
CREATE INDEX IF NOT EXISTS ix_event_status_event_id ON public.event (status_event_id);
