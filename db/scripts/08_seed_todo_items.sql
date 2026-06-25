-- =============================================================================
-- Script: 08_seed_todo_items.sql
-- Sample data for the todo_items table (optional).
-- =============================================================================

INSERT INTO public.todo_items
    (id, title, description, priority, is_completed, created_at)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'Confirm auditorium capacity',
     'Verify capacity with the provider', 3, false, now()),
    ('22222222-2222-2222-2222-222222222222', 'Send invitations',
     'VIP attendee list', 2, false, now())
ON CONFLICT (id) DO NOTHING;
