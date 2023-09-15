export function disableHtmlElementIfWebAuthnUnsupported(elementId) {
    const isSupported = Passwordless.isBrowserSupported();
    if (!isSupported) {
        const element = document.getElementById(elementId);
        if (element) {
            element.disabled = true;
        }
    }
}