-- Blueprint/Table.sql
-- This file contains the SQL schema definitions for the Blueprint application.
-- It includes the creation of tables such as 'customer' and 'messaging_log'.
-- Each table is defined with its respective columns, data types, and constraints.

CREATE TABLE public.customer (
	id serial4 NOT NULL,
	address varchar NULL,
	email varchar NULL,
	phone varchar NULL,
	"name" varchar NULL,
    is_active bit NOT NULL,
	CONSTRAINT customer_pk PRIMARY KEY (id)
);

create table public.messaging_log
(
    id       serial4 NOT NULL,
    topic    varchar,
    group_id varchar,
    app_id   varchar,
    key      varchar,
    payload  varchar,
    retry    varchar,
    error    varchar,
    method   varchar,
    CONSTRAINT messaging_log_pk PRIMARY KEY (id)
);

alter table messaging_log
    add created_at timestamp;

alter table messaging_log
    add updated_at timestamp;

alter table messaging_log
    add status varchar;

CREATE INDEX idx_key ON messaging_log(key);


