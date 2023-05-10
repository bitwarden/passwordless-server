# Admin Console

## Configuration values

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

## Ef migrations
Use the ./migrate.sh shell file to create and/or apply migrations to both Sqlite and Mssql.