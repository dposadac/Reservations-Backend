-- =============================================================================
-- Script: 01_create_table_event_status.sql
-- Type:   Master table (no foreign keys)
-- Source: ER entity "StatusEvent"
-- =============================================================================

CREATE TABLE IF NOT EXISTS public.event_status
(
    id          uuid         NOT NULL DEFAULT gen_random_uuid(),
    status_name varchar(100) NOT NULL,
    description text         NULL,
    CONSTRAINT pk_event_status PRIMARY KEY (id)
);
