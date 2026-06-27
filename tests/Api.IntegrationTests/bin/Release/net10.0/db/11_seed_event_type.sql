-- =============================================================================
-- Script: 11_seed_event_type.sql
-- Sample data for the event_type table.
-- Source: ER entity "EventType"
--
-- NOTE: Los UUID son FIJOS y deben coincidir con EventTypeConverter (capa de
--       infraestructura), que mapea el enum EventType a estos identificadores.
-- =============================================================================

INSERT INTO public.event_type (id, event_type_name)
VALUES
    ('11111111-0000-0000-0000-000000000001', 'conferencia'),
    ('11111111-0000-0000-0000-000000000002', 'taller'),
    ('11111111-0000-0000-0000-000000000003', 'concierto')
ON CONFLICT (id) DO NOTHING;
