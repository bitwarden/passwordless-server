async function createCredential(credentialCreateOptions) {
    credentialCreateOptions.challenge = base64UrlToArrayBuffer(credentialCreateOptions.challenge);
    credentialCreateOptions.user.id = base64UrlToArrayBuffer(credentialCreateOptions.user.id);
    credentialCreateOptions.excludeCredentials?.forEach((cred) => {
        cred.id = base64UrlToArrayBuffer(cred.id);
    });

    const credential = await navigator.credentials.create({
        publicKey: credentialCreateOptions,
    });
    
    const attestationResponse = credential.response;

    return JSON.stringify({
        id: credential.id,
        rawId: arrayBufferToBase64Url(credential.rawId),
        type: credential.type,
        extensions: credential.getClientExtensionResults(),
        response: {
            attestationObject: arrayBufferToBase64Url(attestationResponse.attestationObject),
            clientDataJSON: arrayBufferToBase64Url(attestationResponse.clientDataJSON),
            transports: attestationResponse.getTransports ? attestationResponse.getTransports() : [],
        }
    });
}
