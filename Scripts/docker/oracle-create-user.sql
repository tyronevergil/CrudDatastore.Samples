-- ============================================================
-- Oracle XE bootstrap – creates the application user.
-- Runs automatically via /container-entrypoint-initdb.d/
-- The Oracle.sql schema script runs immediately after.
-- ============================================================

-- gvenzl/oracle-xe sets ORACLE_DATABASE=crudtest which creates
-- a pluggable DB named XEPDB1 with that service name.
-- We create the schema user inside it.

ALTER SESSION SET CONTAINER = XEPDB1;

CREATE USER crudtest IDENTIFIED BY "CrudSamples1"
	DEFAULT TABLESPACE USERS
	TEMPORARY TABLESPACE TEMP
	QUOTA UNLIMITED ON USERS;

GRANT CONNECT, RESOURCE TO crudtest;
