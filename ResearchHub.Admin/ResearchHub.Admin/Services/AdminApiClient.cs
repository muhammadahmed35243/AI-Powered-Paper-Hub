using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ResearchHub.Admin.Services
{
    public class AdminApiClient
    {
        private readonly HttpClient _http = new();
        public string BaseUrl { get; set; } = "http://localhost:5291";
        public string? JwtToken { get; set; }

        private void ApplyAuth()
        {
            _http.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrWhiteSpace(JwtToken))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
        }

        public async Task<string> GetJsonAsync(string path)
        {
            ApplyAuth();
            var response = await _http.GetAsync($"{BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<JsonElement> GetAsync(string path)
        {
            var json = await GetJsonAsync(path);
            return JsonDocument.Parse(json).RootElement;
        }
    }
}
