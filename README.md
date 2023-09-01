# Bitwarden Passwordless.dev Server

Bitwarden Passwordless.dev is a software toolkit that helps developers build FIDO2 WebAuthn passkeys features for seamless authentication flows.

Using Passwordless.dev means there's no need to read extensive W3C specification documentation, determine what cryptography to implement, or worry about managing stored public keys. The team behind Bitwarden will take care of that for you.

The `passwordless-server` project contains the APIs, database, and other core infrastructure items needed for the backend of all passwordless clients. The server project is written in C# using .NET 7 with ASP.NET Core.

## Get started

### Use Passwordless.dev

To get started using Passwordless.dev:

1. [Sign up](https://admin.passwordless.dev/signup/) for a free account here.
2. [Create an application](https://docs.passwordless.dev/guide/get-started.html#create-an-application) in the admin console.
3. Install the Passwordless.dev [JavaScript Client library](https://github.com/passwordless/passwordless-client-js).
4. Start building registration and signin flows for your application. Refer to the [Passwordless.dev documentation](https://docs.passwordless.dev/) for help. Here are some basic examples to get you started:

**Registration**:

```js
// Node.js - Code written for this step should run on your backend.

const payload = {
  "userId": "107fb578-9559-4540-a0e2-f82ad78852f7", // Required. A WebAuthn User Handle, which should be generated by your application. Max. 64 bytes.
  "username": "pjfry@passwordless.dev", // Required. A human readable username used for user authentication, should be chosen by the user.
  // ...For more options, please see the API reference for /register/token.
};

// POST the payload to the Passwordless.dev API using your API private secret.
const apiUrl = "https://v4.passwordless.dev";
const {token} = await fetch(apiUrl + "/register/token", {
    method: "POST",
    body: JSON.stringify(payload),
    headers: {
        "ApiSecret": "myapplication:secret:11f8dd7733744f2596f2a28544b5fbc4",
        "Content-Type": "application/json"
    }
}).then(r => r.json());
```

**Signin**:

```js
// Code written for this step should run on your backend.

// Fetch the verification token from your frontend.
const token = { token: req.query.token };

// POST the verification token to the Passwordless.dev API using your API private secret.
const apiUrl = "https://v4.passwordless.dev";
const response = await fetch(apiurl + "/signin/verify", {
    method: "POST",
    body: JSON.stringify({token}),
    headers: { "ApiSecret": "myapplication:secret:11f8dd7733744f2596f2a28544b5fbc4", "Content-Type": "application/json" }
});

// Cache the API response (see below) to a variable.
const body = await response.json();

// Check the API response for successful verification.
// To see all properties returned by this endpoint, checkout the Backend API Reference for /signin/verify.
if (body.success) {
    console.log("Successfully verified sign-in for user.", body);
    // Set a cookie/userid.
} else {
    console.warn("Sign in failed.", body);
}
```

### Contribute to Passwordless.dev

We welcome code contributions! Please commit any pull requests against the `main` branch. All changes require tests that prove the intended behavior. Please note that large code changes and units of work are less likely to be merged because of the review burden. 

Security audits and feedback are welcome. Please open an issue or email us privately if the report is sensitive in nature. You can read our security policy in the [SECURITY.md](SECURITY.md) file. We also run a program on HackerOne.

No grant of any rights in the trademarks, service marks, or logos of Bitwarden is made (except as may be necessary to comply with the notice requirements as applicable), and use of any Bitwarden trademarks must comply with Bitwarden Trademark Guidelines.

See [CONTRIBUTING.md](CONTRIBUTING.md)

## Docker

### How to build the image

1. Clone the repository
2. Set your working directory to match the root of the cloned files, for example:
   `~/src/passwordless-server`
3. In your terminal execute:
    ```bash
    docker build -t bitwarden/passwordless . -f docker/Dockerfile
    ```

## Need support?

If you need support from the Passwordless.dev team, send us a message at support@passwordless.dev.