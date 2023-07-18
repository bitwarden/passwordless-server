# Server

This is the API server that controls FIDO2 authentication and stores credentials.

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
1. Clone the repository.
2. Open the project in your favorite IDE.
3. Under `src/API`, edit `appsettings.Development.json`.
4. Now it's time to setup our database.
   1. We are using Sqlite by default with file name being `passwordless_dev.db`. If you specify for example a database connection string for the key `ConnectionStrings/sqlite`, then SQlite would be used. For Microsoft SQL Server you can use `ConnectionStrings/mssql`.
   2. Run the `API` project.
   3. In your browser, visit `http://localhost:7001` to execute the database migrations.

## Configuration

```json5
{
  "SENDGRID_API_KEY": "",
  "SALT_TOKEN": "<required base64>",
  "ConnectionStrings": {
    "mssql":"",
    "sqlite":""
  }
}
```
