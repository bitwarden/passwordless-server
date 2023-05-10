# Server

This is the API server that controls FIDO2 authentication and stores credentials.

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
