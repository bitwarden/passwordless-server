async function getCredential(assertionOptions){
    assertionOptions.challenge = base64UrlToArrayBuffer(assertionOptions.challenge);
    assertionOptions.allowCredentials?.forEach((cred) => {
        cred.id = base64UrlToArrayBuffer(cred.id);
    });

    const credential = await navigator.credentials.get({
        publicKey: assertionOptions
    });

    return JSON.stringify({
        id: credential.id,
        rawId: arrayBufferToBase64Url(new Uint8Array(credential.rawId)),
        type: credential.type,
        extensions: credential.getClientExtensionResults(),
        response: {
            authenticatorData: arrayBufferToBase64Url(credential.response.authenticatorData),
            clientDataJSON: arrayBufferToBase64Url(credential.response.clientDataJSON),
            signature: arrayBufferToBase64Url(credential.response.signature)
        }
    });
}