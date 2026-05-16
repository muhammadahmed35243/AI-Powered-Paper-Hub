using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ResearchHub.Web.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly TokenStorage _tokenStorage;

        public ApiClient(HttpClient http, TokenStorage tokenStorage)
        {
            _http = http;
            _tokenStorage = tokenStorage;
        }

        private async Task PrepareAsync()
        {
            _http.DefaultRequestHeaders.Authorization = null;
            var token = await _tokenStorage.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            await PrepareAsync();
            return await _http.GetFromJsonAsync<T>(url);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T body)
        {
            await PrepareAsync();
            return await _http.PostAsJsonAsync(url, body);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string url, T body)
        {
            await PrepareAsync();
            return await _http.PutAsJsonAsync(url, body);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            await PrepareAsync();
            return await _http.DeleteAsync(url);
        }
    }
}
