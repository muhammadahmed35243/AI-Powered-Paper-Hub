using System.Diagnostics;
using System.Text.Json;
using ResearchHub.API.Models;

namespace ResearchHub.API.Services
{
    public class SerpApiService : ISerpApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IApiUsageLogger _usageLogger;

        public SerpApiService(HttpClient httpClient, IConfiguration configuration, IApiUsageLogger usageLogger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _usageLogger = usageLogger;
        }

        public async Task<List<Paper>> SearchAsync(string query, string? field, int? yearFrom, int? yearTo, int maxResults, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["SerpApi:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("SerpAPI key is not configured. Set SerpApi__ApiKey in the .env file at the repository root.");

            var searchQuery = query;
            if (!string.IsNullOrWhiteSpace(field))
                searchQuery += $" {field}";
            if (yearFrom.HasValue || yearTo.HasValue)
            {
                var from = yearFrom ?? 1900;
                var to = yearTo ?? DateTime.UtcNow.Year;
                searchQuery += $" {from}..{to}";
            }

            var url = $"https://serpapi.com/search.json?engine=google_scholar&q={Uri.EscapeDataString(searchQuery)}&api_key={Uri.EscapeDataString(apiKey)}&num={Math.Clamp(maxResults, 1, 20)}";

            var sw = Stopwatch.StartNew();
            var success = false;
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                success = true;
                return ParseResults(json);
            }
            finally
            {
                sw.Stop();
                await _usageLogger.LogAsync("SerpAPI", "google_scholar/search", success, (int)sw.ElapsedMilliseconds);
            }
        }

        private static List<Paper> ParseResults(string json)
        {
            var papers = new List<Paper>();
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("organic_results", out var results))
                return papers;

            foreach (var item in results.EnumerateArray())
            {
                var title = item.TryGetProperty("title", out var t) ? t.GetString() : null;
                if (string.IsNullOrWhiteSpace(title)) continue;

                var link = item.TryGetProperty("link", out var l) ? l.GetString() : null;
                var snippet = item.TryGetProperty("snippet", out var s) ? s.GetString() : null;
                var publicationInfo = item.TryGetProperty("publication_info", out var pi) ? pi : default;

                string? authors = null;
                int? year = null;
                if (publicationInfo.ValueKind == JsonValueKind.Object)
                {
                    if (publicationInfo.TryGetProperty("authors", out var authorsEl))
                    {
                        var authorList = new List<string>();
                        foreach (var a in authorsEl.EnumerateArray())
                        {
                            if (a.TryGetProperty("name", out var name))
                                authorList.Add(name.GetString() ?? "");
                        }
                        authors = string.Join(", ", authorList.Where(x => !string.IsNullOrEmpty(x)));
                    }
                    if (publicationInfo.TryGetProperty("summary", out var summary))
                    {
                        var summaryText = summary.GetString() ?? "";
                        var yearMatch = System.Text.RegularExpressions.Regex.Match(summaryText, @"\b(19|20)\d{2}\b");
                        if (yearMatch.Success && int.TryParse(yearMatch.Value, out var y))
                            year = y;
                    }
                }

                var citedBy = item.TryGetProperty("inline_links", out var links) &&
                              links.TryGetProperty("cited_by", out var cited) &&
                              cited.TryGetProperty("total", out var total) &&
                              total.TryGetInt32(out var count)
                    ? count
                    : 0;

                papers.Add(new Paper
                {
                    Title = title,
                    Authors = authors,
                    PublicationYear = year,
                    Abstract = snippet,
                    Url = link,
                    Source = "Google Scholar",
                    CitationCount = citedBy,
                    FetchedAt = DateTime.UtcNow
                });
            }

            return papers;
        }
    }
}
