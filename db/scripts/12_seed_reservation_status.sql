-- =============================================================================
-- Script: 12_seed_reservation_status.sql
-- Sample data for the reservation_status table.
-- Source: ER entity "StatusReservations"
--
-- NOTE: Los UUID son FIJOS y deben coincidir con ReservationStatusConverter (capa
--       de infraestructura), que mapea el enum ReservationStatus a estos identificadores.
-- =============================================================================

INSERT INTO public.reservation_status (id, status_name, description)
VALUES
    ('33333333-0000-0000-0000-000000000001', 'pendiente_pago', 'Reserva creada, esperando confirmación de pago'),
    ('33333333-0000-0000-0000-000000000002', 'confirmada',     'Pago verificado, reserva activa'),
    ('33333333-0000-0000-0000-000000000003', 'cancelada',      'Reserva cancelada')
ON CONFLICT (id) DO NOTHING;
