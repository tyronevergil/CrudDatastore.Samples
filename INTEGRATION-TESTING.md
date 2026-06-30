# Integration Testing

Unit tests run in-memory by default and require no configuration.  
Integration tests target a real SQL Server and/or Oracle database and are
skipped automatically when connection strings are not supplied.

---

## Prerequisites

- [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop/)
  with **Linux containers** mode enabled.
- All `.NET Framework 4.8.1` build requirements (Visual Studio / MSBuild).

---

## 1 — Start the databases

From the repository root:

```powershell
docker-compose up -d
```

This starts two containers:

| Container                  | Engine           | Port  |
|---------------------------|------------------|-------|
| `cruddatastore-sqlserver`  | SQL Server 2022  | 1433  |
| `cruddatastore-oracle`     | Oracle XE 21c    | 1521  |

The SQL Server entrypoint script creates the `CrudDatastoreTest` database and
runs `Scripts/SqlServer.sql` automatically on first start.  
Oracle runs `Scripts/docker/oracle-create-user.sql` then `Scripts/Oracle.sql`
automatically via its init-directory mechanism.

Wait until both containers are healthy before running tests:

```powershell
docker-compose ps
```

Both `STATUS` columns should show `healthy`.

---

## 2 — Configure connection strings

Edit each project's `App.config` to fill in the appropriate value:

### SQL Server  
Applies to: `SqlClient`, `SqlClientORM`, `SqlClientDopper`, `MultiDbClientORM`

```xml
<add key="SqlClient.ConnectionString"
	 value="Server=localhost,1433;Database=CrudDatastoreTest;User Id=sa;Password=CrudSamples!Pass1;TrustServerCertificate=True;" />
```

### Oracle  
Applies to: `MultiDbClientORM`

```xml
<add key="OracleClient.ConnectionString"
	 value="User Id=crudtest;Password=CrudSamples1;Data Source=localhost:1521/XEPDB1;" />
```

> **Tip**: Leave a value empty to keep that project running in-memory only.  
> Tests use `Assume.That(...)` so they are skipped — not failed — when no
> connection string is configured.

---

## 3 — Run the integration tests

In Visual Studio **Test Explorer**, filter by category:

```
Trait: Category = Integration
```

Or via the command line:

```powershell
# from the repo root
dotnet test --filter "Category=Integration"
```

---

## 4 — Stop / clean up

```powershell
# stop containers, keep data volumes
docker-compose stop

# stop AND remove containers + volumes (full reset)
docker-compose down -v
```

---

## Connection string reference

| Key | Default Docker value |
|-----|---------------------|
| `SqlClient.ConnectionString` | `Server=localhost,1433;Database=CrudDatastoreTest;User Id=sa;Password=CrudSamples!Pass1;TrustServerCertificate=True;` |
| `OracleClient.ConnectionString` | `User Id=crudtest;Password=CrudSamples1;Data Source=localhost:1521/XEPDB1;` |

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| SQL Server container exits immediately | Check `docker logs cruddatastore-sqlserver` — password must meet complexity rules |
| Tests still skip after filling in `App.config` | Ensure the key name matches exactly; rebuild before running |
| Oracle container takes a long time to become healthy | First-start initialisation can take 2–3 minutes; wait for `healthy` status |
| `ORA-01017` login error | User `crudtest` may not have been created; run `docker-compose down -v` then `docker-compose up -d` to reinitialise |
