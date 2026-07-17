-- ============================================================
-- Oracle Free bootstrap – creates the application user.
-- Runs automatically via /container-entrypoint-initdb.d/
-- The Oracle.sql schema script runs immediately after.
-- ============================================================

-- gvenzl/oracle-free creates the default pluggable database
-- FREEPDB1. We switch into that PDB and create the application
-- schema user inside it.
--
-- NOTE:
-- Initialization scripts are executed as SYS. Each script runs
-- in its own session, so Oracle.sql must also switch to FREEPDB1
-- before creating application tables.
-- ============================================================

ALTER SESSION SET CONTAINER = FREEPDB1;

CREATE USER crudtest IDENTIFIED BY "CrudSamples1"
	DEFAULT TABLESPACE USERS
	TEMPORARY TABLESPACE TEMP
	QUOTA UNLIMITED ON USERS;

GRANT CONNECT, RESOURCE TO crudtest;
GRANT CREATE TABLE TO crudtest;
GRANT CREATE VIEW TO crudtest;
GRANT CREATE SEQUENCE TO crudtest;
GRANT CREATE PROCEDURE TO crudtest;