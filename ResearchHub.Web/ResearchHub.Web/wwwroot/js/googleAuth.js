// Same pattern as API wwwroot/google_login_test.html (declarative GIS + global callback)
window.researchHubGoogle = {
    dotNetRef: null,
    initializedClientId: null,

    setDotNetRef: function (dotNetRef) {
        this.dotNetRef = dotNetRef;
    },

    // Called by Google via data-callback on g_id_onload
    handleCredential: function (response) {
        if (this.dotNetRef && response?.credential) {
            this.dotNetRef.invokeMethodAsync('OnGoogleCredential', response.credential);
        }
    },

    // After Blazor renders the sign-in markup, ensure the button appears
    ensureRendered: function (clientId) {
        if (!clientId || !clientId.trim()) {
            return Promise.reject('Google Client ID is missing. Set Google__ClientId in .env at the repo root.');
        }

        return this._waitForGoogle().then(function () {
            if (window.researchHubGoogle.initializedClientId !== clientId) {
                google.accounts.id.initialize({
                    client_id: clientId,
                    callback: window.researchHubGoogle.handleCredential
                });
                window.researchHubGoogle.initializedClientId = clientId;
            }

            // Re-scan for dynamically added g_id_signin (Blazor renders after GIS script loads)
            var signIn = document.querySelector('.g_id_signin');
            if (signIn && !signIn.querySelector('div[role="button"]')) {
                google.accounts.id.renderButton(signIn, {
                    type: 'standard',
                    size: 'large',
                    theme: 'outline',
                    text: 'signin_with',
                    shape: 'rectangular'
                });
            }
        });
    },

    _waitForGoogle: function () {
        return new Promise(function (resolve, reject) {
            if (window.google?.accounts?.id) {
                resolve();
                return;
            }
            var attempts = 0;
            var timer = setInterval(function () {
                attempts++;
                if (window.google?.accounts?.id) {
                    clearInterval(timer);
                    resolve();
                } else if (attempts >= 100) {
                    clearInterval(timer);
                    reject('Google sign-in script did not load. Check network or ad blockers.');
                }
            }, 100);
        });
    }
};
