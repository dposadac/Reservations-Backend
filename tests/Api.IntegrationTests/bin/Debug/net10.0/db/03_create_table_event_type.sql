-- =============================================================================
-- Script: 03_create_table_event_type.sql
-- Type:   Master table (no foreign keys)
-- Source: ER entity "EventType"
-- =============================================================================

CREATE TABLE IF NOT EXISTS public.event_type
(
    id              uuid        NOT NULL DEFAULT gen_random_uuid(),
    event_type_name varchar(50) NOT NULL,
    CONSTRAINT pk_event_type PRIMARY KEY (id)
);
