# Repository Copilot Instructions

## Solution Intent
This repository contains sample applications for the `CrudDatastore` library. The goal is to show a few clear, consistent ways to use the library rather than to create production-ready application code.

## Core Rules
- Keep all projects targeting **.NET Framework 4.8.1** unless the user explicitly asks otherwise.
- Prefer **small, focused sample code** over broad abstractions.
- Preserve the repository's sample-oriented structure and naming conventions.
- Keep the four main sample scenarios easy to understand:
  - `SqlClient`
  - `SqlClientORM`
  - `SqlClientDopper`
  - `MultiDbClientORM`
- When changing adapters or unit-of-work classes, keep the design consistent across sample projects.
- Prefer the unit-of-work owning the connection string and passing factories to adapters when that pattern is already used elsewhere in the repo.
- Do not reintroduce redundant wrappers when `CrudDatastore` already provides equivalent functionality.

## Testing Rules
- Treat build errors as blocking.
- Prefer running the solution build after code changes.
- Integration tests should remain optional and skip cleanly when connection strings are missing.
- If test configuration is updated, keep the README or testing guide aligned.

## Documentation Rules
- Keep documentation focused on how to use the samples.
- When docs overlap, keep a single source of truth and remove duplicates.
- Use concise examples that help readers understand the intended usage quickly.

## Cleanup Rules
- Remove obsolete files only when they are no longer referenced.
- Avoid leaving duplicate adapters, factories, or compatibility shims unless they serve a clear purpose.
- Keep namespaces, project names, and file names consistent with the sample they belong to.

## Validation Expectations
- Build the solution after meaningful changes.
- Run relevant tests when testable code changes.
- Prefer fixes that keep the repository clean and easy to reason about.
