-- =============================================================================
-- Script: 00_create_database.sql
-- Engine: PostgreSQL
-- Purpose: Create the application database.
-- Approach: Database First (schema managed by SQL scripts, NOT EF Core migrations).
--
-- NOTE: Run this script while connected to the 'postgres' maintenance database.
--       CREATE DATABASE cannot run inside a transaction block and does not
--       support IF NOT EXISTS, so drop it manually first if it already exists.
-- =============================================================================

CREATE DATABASE ceiba_reservations
    WITH ENCODING  = 'UTF8'
         TEMPLATE  = template0
         LC_COLLATE = 'en_US.UTF-8'
         LC_CTYPE   = 'en_US.UTF-8';

-- After running this script, reconnect to the 'ceiba_reservations' database
-- and execute the remaining scripts in ascending numeric order (01, 02, ...).
