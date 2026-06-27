-- =============================================================================
-- Script: 10_seed_venue.sql
-- Sample data for the venue table.
-- Source: ER entity "Venue"
-- =============================================================================

INSERT INTO public.venue (venue_name, quantity, city)
SELECT v.venue_name, v.quantity, v.city
FROM (VALUES
    ('Auditorio Central', 200, 'Bogotá'),
    ('Sala Norte',         50, 'Bogotá'),
    ('Arena Sur',         500, 'Medellín')
) AS v(venue_name, quantity, city)
WHERE NOT EXISTS (
    SELECT 1 FROM public.venue ve WHERE ve.venue_name = v.venue_name
);
