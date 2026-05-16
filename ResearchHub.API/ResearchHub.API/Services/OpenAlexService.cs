using System.Diagnostics;
using System.Text.Json;
using ResearchHub.API.Models;

namespace ResearchHub.API.Services
{
    public class OpenAlexService : IOpenAlexService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiUsageLogger _usageLogger;

        public OpenAlexService(HttpClient httpClient, IApiUsageLogger usageLogger)
        {
            _httpClient = httpClient;
            _usageLogger = usageLogger;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ResearchHub/1.0 (mailto:researchhub@example.com)");
        }

        public async Task EnrichPaperAsync(Paper paper, CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();
            var success = false;
            try
            {
                var url = $"https://api.openalex.org/works?search={Uri.EscapeDataString(paper.Title)}&per_page=1";
                var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                    return;

                var work = results[0];
                if (work.TryGetProperty("cited_by_count", out var cited) && cited.TryGetInt32(out var count))
                    paper.CitationCount = Math.Max(paper.CitationCount, count);

                if (work.TryGetProperty("abstract_inverted_index", out var inverted) && inverted.ValueKind == JsonValueKind.Object)
                {
                    var words = new Dictionary<int, string>();
                    foreach (var prop in inverted.EnumerateObject())
                    {
                        if (prop.Value.ValueKind != JsonValueKind.Array) continue;
                        foreach (var pos in prop.Value.EnumerateArray())
                        {
                            if (pos.TryGetInt32(out var index))
                                words[index] = prop.Name;
                        }
                    }
                    if (words.Count > 0 && string.IsNullOrWhiteSpace(paper.Abstract))
                    {
                        paper.Abstract = string.Join(" ", words.OrderBy(kv => kv.Key).Select(kv => kv.Value));
                    }
                }

                if (work.TryGetProperty("publication_year", out var year) && year.TryGetInt32(out var y) && !paper.PublicationYear.HasValue)
                    paper.PublicationYear = y;

                success = true;
            }
            finally
            {
                sw.Stop();
                await _usageLogger.LogAsync("OpenAlex", "works/search", success, (int)sw.ElapsedMilliseconds);
            }
        }
    }
}
