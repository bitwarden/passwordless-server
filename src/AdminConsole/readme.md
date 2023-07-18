# Admin Console
The [Admin Console](https://admin.passwordless.dev/) is your primary GUI for creating and configuring applications, monitoring application usage, and managing billing.

# Getting started
1. Clone the repository.
2. Open the project in your favorite IDE.
3. Under `src/AdminConsole`, edit `appsettings.Development.json`.
4. Now it's time to setup our database.
   1. We are using Sqlite by default with file name being `passwordless_dev.db`. If you specify for example a database connection string for the key `ConnectionStrings/sqlite`, then SQlite would be used. For Microsoft SQL Server you can use `ConnectionStrings/mssql`.
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
Use the ./migrate.sh shell file to create and/or apply migrations to both Sqlite and Mssql.