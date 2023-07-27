# Admin Console
The [Admin Console](https://admin.passwordless.dev/) is your primary GUI for creating and configuring applications, monitoring application usage, and managing billing.

# Requirements
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download).
   - https://learn.microsoft.com/en-us/dotnet/core/install/
- Database (one of the following):
   - Sqlite:
      - (do nothing)
   - Microsoft SQL Server
      - [Docker image](https://hub.docker.com/_/microsoft-mssql-server)
         - [Rosetta](https://support.apple.com/en-us/HT211861) is required for Apple M1 or newer.
      - https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# Getting started
1. (optional) Clone the repository.
2. (optional) Open the project in your favorite IDE.
3. (optional) Under `src/AdminConsole`, edit `appsettings.Development.json`.
4. Now it's time to setup our database.
   1. We are using Sqlite by default with file name being `passwordless_dev.db`. If you specify for example a database connection string for the key `ConnectionStrings:sqlite`, then SQlite would be used. For Microsoft SQL Server you can use `ConnectionStrings:mssql`.
   2. Run the `AdminConsole` project.
   3. In your browser, visit `http://localhost:8001`.
   4. If you see an error saying `A database operation failed while processing the request.`, then click the blue button to apply the database migrations.
5. Configure any other options, see below.

## Configuration

```json5
{
  "Passwordless": {
    // Used for authentication to the Admin Console
    // The ApiURL is used for all Passwordless operations
    "ApiKey": "myAppId:public:123456",
    "ApiSecret": "myAppId:secret:123456",
    "ApiUrl": "<optional>"
  },
  "Mail": {
    // Mail Providers are pluggable. Remove "Postmark" to use the FileProvider
    "Postmark": {
      "ApiKey": "",
      "From": ""
    }
  },
  "ConnectionStrings": {
    // Currently mssql and sqlite are supported.
    // Database is used for all Admin Console operations
    "mssql":"",
    "sqlite":""
  }
}
```

## Entity Framework (EF) migrations
As mentioned in the `Getting Started` section, the Entity Framework migrations are automatically being handled when you run the AdminConsole project.

If you are still interested about learning more about Entity Framework migrations if you're not familiar with .NET, you can learn more about it [here](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli)

You could also use the `./migrate.sh` shell file to create and/or apply migrations to both Sqlite and Mssql.