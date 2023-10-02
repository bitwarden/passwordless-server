# Entity Framework

This document is targeted at contributors of the repo.

## Migrations

### Development
In development environemnts, migrations can automatically be executed from code (visit the http root of each application to trigger it).

### Production.
In product we instead use a [idempotent script](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#idempotent-sql-scripts) to apply migrations via

```bash
#/passwordless-server/
dotnet ef migrations script --idempotent
```

TODO: Add exactly how to run migrations for AdminConsole and Api/Service.


The output from the this command can be executed multiple times, and applies all migrations that are not already applied.
