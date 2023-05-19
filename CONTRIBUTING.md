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
