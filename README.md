# Bitwarden Passwordless.dev Server

[![Build](https://img.shields.io/github/actions/workflow/status/bitwarden/passwordless-server/main.yml?branch=main)](https://github.com/bitwarden/passwordless-server/actions)
[![Coverage](https://img.shields.io/codecov/c/github/bitwarden/passwordless-server/main)](https://codecov.io/gh/bitwarden/passwordless-server)
[![Release](https://img.shields.io/github/release/bitwarden/passwordless-server.svg)](https://github.com/bitwarden/passwordless-server/releases)

Bitwarden Passwordless.dev is a software toolkit that helps developers build FIDO2 WebAuthn passkeys features for seamless authentication flows.

Using Passwordless.dev means there's no need to read extensive W3C specification documentation, determine what cryptography to implement, or worry about managing stored public keys. The team behind Bitwarden will take care of that for you.

The `passwordless-server` project contains the APIs, database, and other core infrastructure items needed for the backend of all passwordless clients.

## Using Passwordless.dev

### Create an application

To get started using Passwordless.dev:

1. [Sign up](https://admin.passwordless.dev/signup/) for a free account here.
2. [Create an application](https://docs.passwordless.dev/guide/get-started.html#create-an-application) in the admin console.
3. Install the Passwordless.dev [JavaScript Client library](https://github.com/passwordless/passwordless-client-js).
4. Start building registration and signin flows for your application. Refer to the [Passwordless.dev documentation](https://docs.passwordless.dev) for help.

### Wire up your backend

You can use Passwordless in conjunction with a variety of different backend platforms — see the [documentation](https://docs.passwordless.dev/guide/backend) for more info. Below is an example of a backend integration using ASP.NET Core and the [Passwordless SDK for .NET](https://github.com/bitwarden/passwordless-dotnet):

```csharp
// Add Passwordless to your service container
services.AddPasswordlessSdk(options =>
{
    options.ApiSecret = "your_api_secret";
});

// ...

// Define the /register endpoint
app.MapGet("/register", async (IPasswordlessClient passwordless, string alias) =>
{
    // Get existing user ID from session or create a new user in your database
    var userId = Guid.NewGuid().ToString();
    
    // Provide the userid and an alias to link to this user
    var payload = new RegisterOptions(userId, alias)
    {
        // Optional: Link this user ID to an alias (e.g. email)
        Aliases = [alias]
    };
    
    try
    {
        var tokenRegistration = await passwordless.CreateRegisterTokenAsync(payload);
    
        // Return this token to the frontend
        return Ok(tokenRegistration);
    }
    catch (PasswordlessApiException e)
    {
        return new JsonResult(e.Details)
        {
            StatusCode = (int?)e.StatusCode,
        };
    }
});

// Define the /signin endpoint
app.MapGet("/signin", async (IPasswordlessClient passwordless, string token) =>
{
    try
    {
        var verifiedUser = await passwordless.VerifyTokenAsync(token);

        // Sign the user in, set a cookie, etc
        return Ok(verifiedUser);
    }
    catch (PasswordlessApiException e)
    {
        return new JsonResult(e.Details)
        {
            StatusCode = (int?)e.StatusCode
        };
    }
});
```

### Wire up your frontend

Finish setting up your registration and signin flows by using the [Passwordless Client](https://github.com/bitwarden/passwordless-client-js) on your frontend. We also provide first-party integrations for several frontend frameworks as well — see the [documentation](https://docs.passwordless.dev/guide/frontend) for more info. Below is a simple example using vanilla JavaScript:

**Install**:

```console
$ npm install @passwordlessdev/passwordless-client
```

**Registration endpoint**:

```js
import Passwordless from '@passwordlessdev/passwordless-client';

// Instantiate a passwordless client using your API public key.
const p = new Passwordless.Client({
    apiKey: "myapplication:public:4364b1a49a404b38b843fe3697b803c8"
});

// Fetch the registration token from the backend.
const backendUrl = "https://localhost:8002";
const registerToken = await fetch(backendUrl + "/register?userId" + userId).then(r => r.json());

// Register the token with the end-user's device.
const { token, error } = await p.register(registerToken);
```

**Signin endpoint**:

```js
import Passwordless from '@passwordlessdev/passwordless-client';

// Instantiate a passwordless client using your API public key.
const p = new Passwordless.Client({
  apiKey: 'myapplication:public:4364b1a49a404b38b843fe3697b803c8'
});

// Generate an authentication token for the user.

// Option 1: Enable browsers to suggest passkeys for any input that has autofill="webauthn" (only works with discoverable passkeys).
const { token, error } = await p.signinWithAutofill();

// Option 2: Enables browsers to suggest passkeys by opening a UI prompt (only works with discoverable passkeys).
const { token, error } = await p.signinWithDiscoverable();

// Option 3: Use an alias specified by the user.
const email = 'pjfry@passwordless.dev';
const { token, error } = await p.signinWithAlias(email);

// Option 4: Use a userId if already known, for example if the user is re-authenticating.
const userId = '107fb578-9559-4540-a0e2-f82ad78852f7';
const { token, error } = await p.signinWithId(userId);

if (error) {
  console.error(error);
  // { errorCode: "unknown_credential", "title": "That credential is not registered with this website", "details": "..."}
}

// Call your backend to verify the token.
const backendUrl = 'https://localhost:8002'; // Your backend
const verifiedUser = await fetch(backendUrl + '/signin?token=' + token).then((r) => r.json());
if (verifiedUser.success === true) {
  // If successful, proceed!
  // verifiedUser.userId = "107fb578-9559-4540-a0e2-f82ad78852f7";
}
```

## Contribute to Passwordless.dev

We welcome code contributions! Please commit any pull requests against the `main` branch. All changes require tests that prove the intended behavior. Please note that large code changes and units of work are less likely to be merged because of the review burden. 

Security audits and feedback are welcome. Please open an issue or email us privately if the report is sensitive in nature. You can read our security policy in the [SECURITY.md](SECURITY.md) file. We also run a program on HackerOne.

No grant of any rights in the trademarks, service marks, or logos of Bitwarden is made (except as may be necessary to comply with the notice requirements as applicable), and use of any Bitwarden trademarks must comply with Bitwarden Trademark Guidelines.

See [CONTRIBUTING.md](CONTRIBUTING.md)

## Self-hosting

See the [self-hosting directory](self-host) for instructions on how to self-host Passwordless.dev.

## Need support?

If you need support from the Passwordless.dev team, send us a message at support@passwordless.dev.
