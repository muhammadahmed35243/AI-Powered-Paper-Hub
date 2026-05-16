window.researchHubAuth = {
    getToken: function () {
        return localStorage.getItem('researchHub_token');
    },
    setToken: function (token) {
        if (token) localStorage.setItem('researchHub_token', token);
        else localStorage.removeItem('researchHub_token');
    }
};
