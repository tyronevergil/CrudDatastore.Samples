# CrudDatastore.Samples

Sample projects demonstrating the different ways to use [CrudDatastore](https://github.com/tyronevergil/CrudDatastore)
— a lightweight data-store abstraction for .NET.

All samples target **.NET Framework 4.8.1** and use **CrudDatastore 2.0.0-preview.1**.

---

## Projects at a glance

| Project | What it shows |
|---------|---------------|
| [`SqlClient`](#sqlclient) | Plain CRUD via `DataContext` backed by SQL Server |
| [`SqlClientORM`](#sqlclientorm) | ORM-style CRUD with navigation properties and shared transactions |
| [`SqlClientDopper`](#sqlclientdopper) | Dapper-style extension methods directly on `SqlConnection` |
| [`MultiDbClientORM`](#multidbclientorm) | ORM-style CRUD spanning SQL Server **and** Oracle in one unit of work |

All projects share a common adapter library (`CrudDatastore.Samples.Adapters`) that provides
the SQL Server and Oracle `DelegateCrudAdapter<T>` implementations.

---

## Running the tests

Each project contains two test classes:

| Class | Backend | When it runs |
|-------|---------|--------------|
| `UnitTest` | In-memory list (`InMemoryListExtensions`) | Always — no config needed |
| `IntegrationTest` | Real database | Only when a connection string is set in `App.config` |

Integration tests skip automatically (via NUnit `Assume.That`) when the connection string is empty,
so the default test run is always green without any database.

To run integration tests against real databases, see **[INTEGRATION-TESTING.md](INTEGRATION-TESTING.md)**.

---

## SqlClient

> `CrudDatastore.Samples.SqlClient`

The simplest pattern. A `DataContext` wraps a `UnitOfWorkBase` that registers plain
`DataStore<T>` instances backed by `SqlClientCrudAdapter<T>`.

**Unit of work setup**

```csharp
// In-memory (default — no connection string required)
DataContext.Factory()

// SQL Server
DataContext.Factory("Server=localhost;Database=CrudDatastoreTest;...")
```

**CRUD**

```csharp
using (var context = DataContext.Factory())
{
	// Create
	var person = new Person { Firstname = "Pauline", Lastname = "Koch" };
	context.Add(person);
	context.SaveChanges();
	// person.PersonId is now populated

	// Read
	var all    = context.Find<Person>(p => true);
	var single = context.FindSingle<Person>(p => p.PersonId == person.PersonId);

	// Update
	single.Firstname = "Paula";
	context.Update(single);
	context.SaveChanges();

	// Delete
	context.Delete(single);
	context.SaveChanges();
}
```

**Key files**

```
SqlClient/
├── Entities/
│   ├── Person.cs
│   └── Identification.cs
├── InMemoryUnitOfWork.cs   ← seeds test data via InMemoryListExtensions
├── SqlClientUnitOfWork.cs  ← wires SqlClientCrudAdapter<T> per entity
├── DataContext.cs          ← Factory() / Factory(connectionString)
├── UnitTest.cs             ← in-memory tests
└── IntegrationTest.cs      ← SQL Server tests (skipped when not configured)
```

---

## SqlClientORM

> `CrudDatastore.Samples.SqlClientORM`

Extends the basic pattern with **navigation properties** and a **shared transaction**
across all stores in a single `Commit()`.

#### What is the shared transaction?

In `SqlClient`, each adapter opens and closes its own `SqlConnection` per command.
That means a `SaveChanges()` that writes to multiple tables uses separate, independent
connections — if the second write fails, the first one has already committed.

`SqlClientORM` solves this by making the `SqlClientUnitOfWork` itself implement
`ISqlCommandFactory`. When `Commit()` is called it opens **one** `SqlConnection`,
starts a `SqlTransaction`, and passes both to every adapter via `CreateSqlCommand()`.
All writes share that single connection and transaction, so they all commit or all roll
back together.

```
SaveChanges()
  └─ Commit()
	   ├─ open one SqlConnection
	   ├─ BeginTransaction()
	   ├─ INSERT/UPDATE/DELETE Person    ← same connection + transaction
	   ├─ INSERT/UPDATE/DELETE Identification ← same connection + transaction
	   └─ Commit()  (or Rollback() on error)
```

Outside of `Commit()` (reads, individual adapter calls) each command still gets its
own short-lived connection — same as `SqlClient`.

**Unit of work setup**

```csharp
// In-memory (default)
DataContext.Factory()

// SQL Server (shared transaction on commit)
DataContext.Factory("Server=localhost;Database=CrudDatastoreTest;...")
```

**CRUD with navigation properties**

```csharp
using (var context = DataContext.Factory())
{
	// Create with related entities
	var person = new Person
	{
		Firstname = "Pauline",
		Lastname  = "Koch",
		Identifications = new List<Identification>
		{
			new Identification { Type = Identification.Types.SSN, Number = "222-222-2222" }
		}
	};
	context.Add(person);
	context.SaveChanges();

	// Read — Identifications is populated automatically
	var loaded = context.FindSingle<Person>(p => p.PersonId == person.PersonId);
	var ids    = loaded.Identifications; // List<Identification>

	// Update — add another identification
	loaded.Identifications.Add(new Identification { Type = Identification.Types.TIN, Number = "333-333" });
	context.Update(loaded);
	context.SaveChanges();

	// Delete — cascades to child identifications
	context.Delete(loaded);
	context.SaveChanges();
}
```

**Key files**

```
SqlClientORM/
├── Entities/
│   ├── Person.cs             ← has List<Identification> Identifications
│   └── Identification.cs
├── InMemoryUnitOfWork.cs     ← seeds data and maps navigation property
├── SqlClientUnitOfWork.cs    ← implements ISqlCommandFactory; shared tx on Commit()
├── DataContext.cs
├── UnitTest.cs
└── IntegrationTest.cs
```

---

## SqlClientDopper

> `CrudDatastore.Samples.SqlClientDopper`

A **Dapper-style** API — extension methods hang directly off `SqlConnection` with no
`DataContext` or unit-of-work ceremony. Useful when you already have an open connection
and want lightweight CRUD without a full context.

All methods accept an optional `SqlTransaction` for transactional writes.

**Usage**

```csharp
using (var connection = new SqlConnection("Server=localhost;Database=CrudDatastoreTest;..."))
{
	// Create
	var person = new Person { Firstname = "Pauline", Lastname = "Koch" };
	connection.Add(person);
	// person.PersonId is now populated

	// Read
	var all    = connection.Find<Person>(p => true);
	var single = connection.FindSingle<Person>(p => p.PersonId == person.PersonId);

	// Update
	single.Firstname = "Paula";
	connection.Update(single);

	// Delete
	connection.Delete(single);
}
```

**With a transaction**

```csharp
using (var connection = new SqlConnection("..."))
using (var tx = connection.BeginTransaction())
{
	connection.Add(person, tx);
	connection.Update(other, tx);
	tx.Commit();
}
```

**Key files**

```
SqlClientDopper/
├── Entities/
│   └── Person.cs
├── SqlClientExtensions.cs   ← Find / FindSingle / Add / Update / Delete / Execute
├── IntegrationTest.cs       ← SQL Server tests (skipped when not configured)
```

> `SqlClientDopper` has no `UnitTest.cs` because the extension methods require a real
> `SqlConnection`; all tests are integration tests.

---

## MultiDbClientORM

> `CrudDatastore.Samples.MultiDbClientORM`

Demonstrates a single unit of work that spans **two databases** — `Person` rows live in
SQL Server and `Identification` rows live in Oracle. Navigation properties work across
the database boundary transparently.

Commit is sequential (SQL Server first, then Oracle). See the comment in
`MultiDbClientUnitOfWork.cs` for how to swap in a `TransactionScope` + MSDTC if you need
true two-phase commit on Windows.

**Unit of work setup**

```csharp
// In-memory (default — both stores in memory)
DataContext.Factory()

// SQL Server + Oracle
DataContext.Factory(
	"Server=localhost,1433;Database=CrudDatastoreTest;User Id=sa;Password=...;",
	"User Id=crudtest;Password=...;Data Source=localhost:1521/XEPDB1;"
)
```

**CRUD**

```csharp
using (var context = DataContext.Factory(sqlCs, oracleCs))
{
	// Create — Person → SQL Server, Identification → Oracle
	var person = new Person
	{
		Firstname = "Pauline",
		Lastname  = "Koch",
		Identifications = new List<Identification>
		{
			new Identification { Type = 1, Number = "222-222-2222" }
		}
	};
	context.Add(person);
	context.SaveChanges();

	// Read — navigation property resolves cross-database
	var loaded = context.FindSingle<Person>(p => p.PersonId == person.PersonId);
	var ids    = loaded.Identifications;

	// Update / Delete work the same as SqlClientORM
}
```

**Key files**

```
MultiDbClientORM/
├── Entities/
│   ├── Person.cs
│   └── Identification.cs
├── InMemoryUnitOfWork.cs        ← both stores in memory
├── MultiDbClientUnitOfWork.cs   ← SQL Server + Oracle, sequential commit
├── DataContext.cs               ← Factory() / Factory(sqlCs, oracleCs)
├── UnitTest.cs
└── IntegrationTest.cs
```

---

## Integration testing with Docker

The repo ships a `docker-compose.yml` that starts SQL Server 2022 and Oracle XE 21c
locally with the schema pre-loaded.

```powershell
docker-compose up -d   # start both databases
docker-compose ps      # wait until STATUS = healthy
```

Then fill in the connection strings in each project's `App.config` and run the
`[Category("Integration")]` tests. Full instructions: **[INTEGRATION-TESTING.md](INTEGRATION-TESTING.md)**.
