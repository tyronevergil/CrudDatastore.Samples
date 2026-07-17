-- ============================================================
-- Oracle Free schema for CrudDatastore.Samples integration tests
-- Runs automatically via /container-entrypoint-initdb.d/
--
-- This script executes as SYS during Oracle initialization.
-- It switches to the application PDB and sets CRUDTEST as the
-- current schema before creating application tables.
--
-- Used by:
--   CrudDatastore.Samples.MultiDbClientORM -> "Identifications"
-- ============================================================

ALTER SESSION SET CONTAINER = FREEPDB1;

ALTER SESSION SET CURRENT_SCHEMA = CRUDTEST;

CREATE TABLE "Identifications" (
    "IdentificationId" NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "PersonId"         NUMBER        NOT NULL,
    "Type"             NUMBER        NOT NULL,
    "Number"           NVARCHAR2(50) NOT NULL
);