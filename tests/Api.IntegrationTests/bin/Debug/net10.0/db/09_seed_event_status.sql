-- =============================================================================
-- Script: 09_seed_event_status.sql
-- Sample data for the event_status table.
-- Source: ER entity "StatusEvent"
--
-- NOTE: Los UUID son FIJOS y deben coincidir con EventStatusConverter (capa de
--       infraestructura), que mapea el enum EventStatus a estos identificadores.
-- =============================================================================

INSERT INTO public.event_status (id, status_name, description)
VALUES
    ('22222222-0000-0000-0000-000000000001', 'activo',     NULL),
    ('22222222-0000-0000-0000-000000000002', 'cancelado',  NULL),
    ('22222222-0000-0000-0000-000000000003', 'completado', NULL)
ON CONFLICT (id) DO NOTHING;
