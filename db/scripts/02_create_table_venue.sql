-- =============================================================================
-- Script: 02_create_table_venue.sql
-- Type:   Master table (no foreign keys)
-- Source: ER entity "Venue"
-- =============================================================================

CREATE TABLE IF NOT EXISTS public.venue
(
    id         uuid         NOT NULL DEFAULT gen_random_uuid(),
    venue_name varchar(100) NOT NULL,
    quantity   integer      NOT NULL DEFAULT 0,
    city       varchar(50)  NULL,
    CONSTRAINT pk_venue PRIMARY KEY (id),
    CONSTRAINT ck_venue_quantity CHECK (quantity >= 0)
);
