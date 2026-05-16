using Microsoft.JSInterop;

namespace ResearchHub.Web.Services
{
    public class TokenStorage
    {
        private readonly IJSRuntime _js;

        public TokenStorage(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string?> GetTokenAsync() =>
            await _js.InvokeAsync<string?>("researchHubAuth.getToken");

        public async Task SetTokenAsync(string? token) =>
            await _js.InvokeVoidAsync("researchHubAuth.setToken", token);
    }
}
