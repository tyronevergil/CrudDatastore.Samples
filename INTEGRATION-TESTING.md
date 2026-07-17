# Integration Testing Guide

This guide explains how to run integration tests for the CrudDatastore.Samples project.

## Quick Start (5 Minutes)

### 1. Start Docker Containers

```powershell
docker-compose up -d
```

This launches:
- **SQL Server 2022** on `localhost:1433` (database: `CrudDatastoreTest`)
- **Oracle Free 23** on `localhost:1521` (service: `FREEPDB1`, user: `crudtest`)

### 2. Wait for Health Checks

Check container status:
```powershell
docker-compose ps
```

All services should show `healthy` status before running tests (usually 30-60 seconds).

### 3. Connection Strings Already Configured

The `App.config` files in each project have been pre-configured with Docker connection strings:

**SQL Server (SqlClient, SqlClientORM, SqlClientDopper):**
```xml
<add key="SqlClient.ConnectionString" 
	 value="Server=localhost,1433;Database=CrudDatastoreTest;User Id=sa;Password=CrudSamples!Pass1;TrustServerCertificate=True;" />
```

**Oracle Free (MultiDbClientORM):**
```xml
<add key="OracleClient.ConnectionString" 
	 value="User Id=crudtest;Password=CrudSamples1;Data Source=localhost:1521/FREEPDB1;" />
```

### 4. Run Tests

**Option A: Visual Studio Test Explorer**
- Open **Test Explorer** (Test → Windows → Test Explorer)
- Click **Run All Tests**
- Integration tests will now execute (no longer skipped)

**Option B: Command Line**
```powershell
cd C:\Users\tyronevergil\source\repos\CrudDatastore.Samples
dotnet test
```

## Prerequisites

- [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop/)
  with **Linux containers** mode enabled.
- All `.NET Framework 4.8.1` build requirements (Visual Studio / MSBuild).

## Test Structure

Each project has two test classes:

| Class | Type | Connection | Runs When |
|-------|------|-----------|-----------|
| `UnitTest.cs` | In-memory | None required | Always ✅ |
| `IntegrationTest.cs` | Database | Requires config | Connection string present ✅ |

**Unit tests** use `DataContext.Factory()` to work in-memory with sample data.

**Integration tests** use `DataContext.Factory(connectionString)` and:
- Connect to real databases
- Create/Read/Update/Delete actual records
- Automatically skip if connection string is empty or missing

## Project Coverage

| Project | SQL Server | Oracle | Tests |
|---------|-----------|--------|-------|
| **SqlClient** | ✅ | — | Unit + Integration (SQL Server) |
| **SqlClientORM** | ✅ | — | Unit + Integration (SQL Server) |
| **SqlClientDopper** | ✅ | — | Unit + Integration (SQL Server) |
| **MultiDbClientORM** | ✅ | ✅ | Unit + Integration (SQL Server + Oracle) |

## Stopping Containers

```powershell
docker-compose down
```

To also remove database volumes:
```powershell
docker-compose down -v
```

## Starting SQL Server Alone

If you only need SQL Server (without Oracle), start just the SQL Server container:

```powershell
# Start only SQL Server
docker-compose up -d sqlserver

# Check status
docker-compose ps

# Wait for health check to pass (usually 30-60 seconds)
Start-Sleep -Seconds 60
docker-compose ps
```

Once the SQL Server container shows `healthy`, initialize the database if needed:

```powershell
# Create the database
docker exec cruddatastore-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CrudSamples!Pass1" -C -Q "CREATE DATABASE CrudDatastoreTest"

# Run schema script
docker exec cruddatastore-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CrudSamples!Pass1" -C -d CrudDatastoreTest -i /scripts/SqlServer.sql

# Verify tables exist
docker exec cruddatastore-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CrudSamples!Pass1" -C -d CrudDatastoreTest -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"
```

Then run the SqlClient, SqlClientORM, or SqlClientDopper integration tests.

## Troubleshooting

### "Connection refused" error
- Containers haven't finished starting. Wait 30-60 seconds and retry.
- Check: `docker-compose ps` — services must show `healthy`.
- If SQL Server container exits immediately, check for line ending issues in startup script (see **Line Endings Caveat** below).

### "Database 'CrudDatastoreTest' does not exist"
- SQL Server initialization script didn't run or failed silently.
- Check Docker logs:
  ```powershell
  docker logs cruddatastore-sqlserver | Select-String "Schema initialised"
  ```
- If not found, manually initialize:
  ```powershell
  docker exec cruddatastore-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CrudSamples!Pass1" -C -Q "CREATE DATABASE CrudDatastoreTest"
  docker exec cruddatastore-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CrudSamples!Pass1" -C -d CrudDatastoreTest -i /scripts/SqlServer.sql
  ```

### "Unable to locate ODP.NET configuration"
- The MultiDbClientORM project initializes Oracle provider automatically. Ensure `Oracle.ManagedDataAccess` NuGet package is installed.

### Tests still skipping
- Verify connection strings aren't empty in `App.config`
- Check that containers are running: `docker-compose ps`

### "ExecuteScalar requires an open and available Connection"
- Connection management issue in SQL Server adapters. This should be fixed in current version.
- If you encounter this, verify you have the latest SqlClientUnitOfWork with proper connection lifecycle management (see **Connection Management Caveat** below).



## Expected Test Output

When running successfully, you should see:

```
========== Test run finished: 20 Tests (20 Passed, 0 Failed) ==========
```

- **8 Unit tests** (in-memory, always pass) ✅
- **12 Integration tests** (database, now executing) ✅

## Continuous Integration (CI/CD)

For automated testing pipelines:

1. Ensure Docker/Docker Compose is available in CI environment
2. Initialize Docker containers before test run: `docker-compose up -d`
3. Wait for health checks to pass
4. Run tests: `dotnet test`
5. Clean up: `docker-compose down`

See CI/CD pipeline documentation for your specific platform (GitHub Actions, Azure Pipelines, etc.).

## Oracle Free 23 Details

The CrudDatastore.Samples uses the `gvenzl/oracle-free:23-slim` Docker image, which provides Oracle Database Free Edition 23c.

**Default PDB:** `FREEPDB1`  
**Default User:** `sys`  
**App User:** `crudtest` (created during initialization)

For more details, see [gvenzl/oracle-free on Docker Hub](https://hub.docker.com/r/gvenzl/oracle-free).

## Line Endings Caveat

SQL Server initialization scripts must use **LF (Unix) line endings**, not CRLF (Windows). The repository includes `.gitattributes` to enforce this, but if you clone on Windows with auto-conversion enabled, you may need to manually convert:

```powershell
# Fix line endings if needed
dos2unix Scripts/docker/sqlserver-entrypoint.sh
```

## Connection Management Caveat

The `SqlClientCrudAdapter` uses a shared connection/transaction model where:
- One connection per `UnitOfWork` instance
- One transaction per `ExecuteAsync` call
- Connections are kept open for adapter reuse

If you add new adapters or modify connection lifecycle, ensure:
1. Connections are properly disposed
2. Transactions are committed before the connection closes
3. Multiple `ExecuteAsync` calls within one `UnitOfWork` share the same transaction
