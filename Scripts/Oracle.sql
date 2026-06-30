-- ============================================================
-- Oracle schema for CrudDatastore.Samples integration tests
-- Run once against your test schema before running integration tests
--
-- Used by:
--   CrudDatastore.Samples.MultiDbClientORM -> "Identifications"
-- ============================================================

-- Identifications table
CREATE TABLE "Identifications" (
	"IdentificationId" NUMBER        GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	"PersonId"         NUMBER        NOT NULL,
	"Type"             NUMBER        NOT NULL,
	"Number"           NVARCHAR2(50) NOT NULL
);
