# CrudDatastore.Samples

## Baseline snapshot

- A `v1` branch has been created from current `master` so you have a stable baseline before the package migration.

## Migration path to `CrudDatastore 2.0.0-preview.1` (targeting `.NET Framework 4.8.1`)

### Applied changes

1. **Baseline branch**
   - `v1` branch was created from current `master`.

2. **Project target framework**
   - All projects were retargeted to `v4.8.1`:
	 - `CrudDatastore.SqlClient`
	 - `CrudDatastore.SqlClientORM`
	 - `CrudDatastore.MultiDbClientORM`
	 - `CrudDatastore.SqlClientDopper`

3. **Package migration**
   - Updated `CrudDatastore` to `2.0.0-preview.1` in all `packages.config` files.
   - Updated `.csproj` `HintPath` references to:
	 - `..\packages\CrudDatastore.2.0.0-preview.1\lib\net481\CrudDatastore.dll`

4. **Restore status**
   - NuGet restore completed successfully via MSBuild (`/t:Restore /p:RestorePackagesConfig=true`).

### API migration status

The v2 API migration pass has been applied and the solution now builds successfully.

Key updates completed:
- Added `CrudDatastore.Framework` namespace usage where `DelegateCrudAdapter<T>`, `DataStore<T>`, and `UnitOfWorkBase` are used.
- Updated command execution in `SqlClientDopper/SqlClientExtensions.cs` from legacy `Command`/`IDataCommand` patterns to `Action`/`ICommand` and `SatisfyingActionFrom(...)`.
- Removed legacy `Execute(...)` extension methods tied to removed `DataContextBase` command APIs.

### Validation

- Package restore: successful
- Solution build: successful
- Next recommended validation: run project test flows (Create/Update/Delete/Find) against your target databases.

## Project samples

> These examples are aligned with the project `Test.cs` files and show the intended usage pattern.

### `CrudDatastore.MultiDbClientORM`

```csharp
using (var context = DataContext.Factory())
{
	var person = new Person
	{
		Firstname = "Pauline",
		Lastname = "Koch",
		Identifications = new List<Identification>
		{
			new Identification { Type = 1, Number = "222-222-2222" }
		}
	};

	context.Add(person);
	context.SaveChanges();

	var loaded = context.FindSingle<Person>(p => p.PersonId == person.PersonId);
}
```

### `CrudDatastore.SqlClient`

```csharp
using (var context = DataContext.Factory())
{
	var person = new Person
	{
		Firstname = "Pauline",
		Lastname = "Koch"
	};

	context.Add(person);

	var people = context.Find<Person>(p => p.Lastname == "Koch");
}
```

### `CrudDatastore.SqlClientDopper`

```csharp
using (var connection = new SqlConnection("<connectionstring>"))
{
	var person = new Person
	{
		Firstname = "Pauline",
		Lastname = "Koch"
	};

	connection.Add(person);

	var loaded = connection.FindSingle<Person>(p => p.PersonId == person.PersonId);
}
```

### `CrudDatastore.SqlClientORM`

```csharp
using (var context = DataContext.Factory())
{
	var person = new Person
	{
		Firstname = "Pauline",
		Lastname = "Koch",
		Identifications = new List<Identification>
		{
			new Identification
			{
				Type = Identification.Types.SSN,
				Number = "222-222-2222"
			}
		}
	};

	context.Add(person);
	context.SaveChanges();

	var people = context.Find<Person>(p => p.PersonId > 0);
}
```

## Next step

- Optional: run and verify all sample tests with real SQL Server/Oracle connection strings to validate runtime behavior under `CrudDatastore 2.0.0-preview.1`.
