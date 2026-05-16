using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace ResearchHub.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly TokenStorage _tokenStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient http, TokenStorage tokenStorage, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _tokenStorage = tokenStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> LoginWithGoogleAsync(string idToken)
        {
            _http.DefaultRequestHeaders.Authorization = null;
            var response = await _http.PostAsJsonAsync("auth/google-login", new { idToken });
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? token = null;
            if (json.TryGetProperty("token", out var t)) token = t.GetString();
            else if (json.TryGetProperty("Token", out var t2)) token = t2.GetString();
            if (string.IsNullOrEmpty(token)) return false;

            await _tokenStorage.SetTokenAsync(token);
            ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
            return true;
        }

        public async Task LogoutAsync()
        {
            try { await _http.PostAsync("auth/logout", null); } catch { /* ignore */ }
            await _tokenStorage.SetTokenAsync(null);
            ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
        }

        public static ClaimsPrincipal CreatePrincipalFromToken(string token)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
    }
}
