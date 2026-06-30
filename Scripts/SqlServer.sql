-- ============================================================
-- SQL Server schema for CrudDatastore.Samples integration tests
-- Run once against your test database before running integration tests
--
-- Used by:
--   CrudDatastore.Samples.SqlClient       -> People, Identifications
--   CrudDatastore.Samples.SqlClientORM    -> People, Identifications
--   CrudDatastore.Samples.SqlClientDopper -> People
--   CrudDatastore.Samples.MultiDbClientORM-> People  (Identifications in Oracle)
-- ============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'People' AND xtype = 'U')
CREATE TABLE [People] (
	[PersonId]  INT           IDENTITY(1,1) NOT NULL,
	[Firstname] NVARCHAR(100) NOT NULL,
	[Lastname]  NVARCHAR(100) NOT NULL,
	CONSTRAINT [PK_People] PRIMARY KEY ([PersonId])
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Identifications' AND xtype = 'U')
CREATE TABLE [Identifications] (
	[IdentificationId] INT          IDENTITY(1,1) NOT NULL,
	[PersonId]         INT          NOT NULL,
	[Type]             INT          NOT NULL,
	[Number]           NVARCHAR(50) NOT NULL,
	CONSTRAINT [PK_Identifications] PRIMARY KEY ([IdentificationId])
);
