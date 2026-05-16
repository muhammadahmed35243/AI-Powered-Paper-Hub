using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ResearchHub.Web.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly TokenStorage _tokenStorage;

        public CustomAuthStateProvider(TokenStorage tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _tokenStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var user = AuthService.CreatePrincipalFromToken(token);
                if (user.Identity?.IsAuthenticated == true)
                    return new AuthenticationState(user);
            }
            catch { /* expired or invalid */ }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void NotifyAuthenticationStateChanged() =>
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
