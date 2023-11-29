function base64UrlToArrayBuffer(base64UrlString) {
    // improvement: Remove BufferSource-type and add proper types upstream
    if (typeof base64UrlString !== 'string') {
        const msg = "Cannot convert from Base64Url to ArrayBuffer: Input was not of type string";
        console.error(msg, base64UrlString);
        throw new TypeError(msg);
    }

    const base64Unpadded = base64UrlToBase64(base64UrlString);
    const paddingNeeded = (4 - (base64Unpadded.length % 4)) % 4;
    const base64Padded = base64Unpadded.padEnd(base64Unpadded.length + paddingNeeded, "=");

    const binary = window.atob(base64Padded);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }

    return bytes;
}

function arrayBufferToBase64Url(buffer) {
    const uint8Array = (() => {
        if (Array.isArray(buffer)) return Uint8Array.from(buffer);
        if (buffer instanceof ArrayBuffer) return new Uint8Array(buffer);
        if (buffer instanceof Uint8Array) return buffer;

        const msg = "Cannot convert from ArrayBuffer to Base64Url. Input was not of type ArrayBuffer, Uint8Array or Array";
        console.error(msg, buffer);
        throw new Error(msg);
    })();

    let string = '';
    for (let i = 0; i < uint8Array.byteLength; i++) {
        string += String.fromCharCode(uint8Array[i]);
    }

    const base64String = window.btoa(string);
    return base64ToBase64Url(base64String);
}

function base64UrlToBase64(base64Url) {
    return base64Url.replace(/-/g, '+').replace(/_/g, '/');
}

function base64ToBase64Url(base64) {
    return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=*$/g, '');
}
