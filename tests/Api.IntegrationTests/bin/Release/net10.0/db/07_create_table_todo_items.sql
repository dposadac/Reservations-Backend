-- =============================================================================
-- Script: 07_create_table_todo_items.sql
-- Type:   Sample/baseline table (no foreign keys)
-- Purpose: Backs the TODO example used to showcase the Clean Architecture stack.
--          Must stay aligned with TodoItemConfiguration.cs.
-- =============================================================================

CREATE TABLE IF NOT EXISTS public.todo_items
(
    id               uuid          NOT NULL,
    title            varchar(200)  NOT NULL,
    description      varchar(2000) NULL,
    priority         integer       NOT NULL DEFAULT 0,
    is_completed     boolean       NOT NULL DEFAULT false,
    completed_at     timestamptz   NULL,
    created_at       timestamptz   NOT NULL,
    last_modified_at timestamptz   NULL,
    CONSTRAINT pk_todo_items PRIMARY KEY (id),
    CONSTRAINT ck_todo_items_priority CHECK (priority BETWEEN 0 AND 3)
);

CREATE INDEX IF NOT EXISTS ix_todo_items_is_completed
    ON public.todo_items (is_completed);
