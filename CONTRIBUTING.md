We welcome code contributions! Please commit any pull requests against the `main` branch. All changes require tests that prove the intended behavior. Please note that large code changes and units of work are less likely to be merged because of the review burden. 

Security audits and feedback are welcome. Please open an issue or email us privately if the report is sensitive in nature. You can read our security policy in the SECURITY.md file. We also run a program on HackerOne.

No grant of any rights in the trademarks, service marks, or logos of Bitwarden is made (except as may be necessary to comply with the notice requirements as applicable), and use of any Bitwarden trademarks must comply with Bitwarden Trademark Guidelines.

## Start the project

* You need .NET 7 SDK installed.
* We recommend either VSCode, Rider or Visual studio.

When developing, you need to to run both of the apps: `Api` and `AdminConsole`.

In a terminal, run:

* dotnet watch --project src/Api
* dotnet watch --project src/AdminConsole

This will launch the apps:

* http:/localhost:7001 - API
* https:/localhost:7002 - API
* http:/localhost:8001 - AdminConsole
* https:/localhost:8002 - AdminConsole

If you're using the `https` endpoints, you might need to trust the cert. Using the HTTP endpoints is fine for local development.
Do note that if not on `localhost`, WebAuthn will not work if not on `https`.

### Database

During development two SQLite databases are used:

* Api/passwordless_dev.db
* AdminConsole/adminconsole_dev.db

Both apps will allows you to automatically migrate the database and add seeded development data.
Please visit both the apps in your browser, I recommend you start with the API app first.

#### Migrations

Database changes are pretty straightforward thanks to EF migrations.

##### Important Note:
When generating migrations, you can only have the connection string for your desired database platform in the 
`appsettings.Development.json`.  If you have both, it will either error when attempting the migration, or pretend it did 
what you wanted and not actually do anything.  You would have to change the code for it to run both migrations at once.

##### For AdminConsole:
1. Make your desired changes to the entities that define the database tables for the `ConsoleDbContext` located in 
`AdminConsole/Db`
2. Open the `AdminConsole` directory in your terminal of choice.
3. Run the following command, `dotnet ef migrations add <friendly-name-of-change> --context <name-of-db-context>`
   1. `friendly-name-of-change` should be a description of the change to the schema 
   2. `name-of-db-context` is the name of the database provider you're generating the migration for 
      1. `MssqlConsoleDbContext`
      2. `SqliteConsoleDbContext`
4. Make sure you create the migration for both `Mssql` and `Sqlite`
5. Run `dotnet ef migrations update` to apply the migration to your local databases.

`Api` works a little differently than `AdminConsole`. The application is the `Api` project while the migrations and 
database definitions are stored in `Service`.

##### For Api:
1. Make your desired changes to the entities that define the database tables for the `DbGlobalContext` located in 
`Service/Storage/Ef`
2. Open the `Api` directory in your terminal of choice.
3. Run the following command, `dotnet ef migrations add <friendly-name-of-change> --context <name-of-db-context> 
--project ../Service`
   1. `friendly-name-of-change` should be a description of the change to the schema
   2. `name-of-db-context` is the name of the database provider you're generating the migration for 
      1. `DbGlobalMsSqlContext`
      2. `DbGlobalSqliteContext`
4. Make sure you create the migration for both `Mssql` and `Sqlite`
5. Run the `Api` project and visit `localhost:7000` which will automatically apply any new migrations.

##### Remember:
If you need to undo a migration you just applied, you can run `dotnet ef migrations remove` to undo the last migration 
applied.