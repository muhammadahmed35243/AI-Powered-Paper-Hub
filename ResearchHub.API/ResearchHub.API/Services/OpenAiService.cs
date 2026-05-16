using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ResearchHub.API.Models;

namespace ResearchHub.API.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IApiUsageLogger _usageLogger;

        public OpenAiService(HttpClient httpClient, IConfiguration configuration, IApiUsageLogger usageLogger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _usageLogger = usageLogger;
        }

        public async Task<PaperSummary> GenerateSummaryAsync(Paper paper, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key is not configured. Set OpenAI__ApiKey in the .env file at the repository root.");

            var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";
            var prompt = $"""
                Summarize this academic paper for a researcher. Return JSON only with keys:
                summaryText (2-3 sentences), keyFindings (bullet list as plain text), methodology (1-2 sentences).

                Title: {paper.Title}
                Authors: {paper.Authors}
                Year: {paper.PublicationYear}
                Abstract: {paper.Abstract ?? "N/A"}
                """;

            var content = await CallChatAsync(apiKey, model, prompt, cancellationToken);
            var parsed = TryParseSummaryJson(content);

            return new PaperSummary
            {
                PaperId = paper.PaperId,
                SummaryText = parsed.SummaryText,
                KeyFindings = parsed.KeyFindings,
                Methodology = parsed.Methodology,
                GeneratedByAi = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<string> ExplainRecommendationAsync(string topic, Paper paper, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return $"Recommended because it relates to your interest in {topic}.";

            var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";
            var prompt = $"In one short sentence, explain why '{paper.Title}' is recommended for someone researching '{topic}'.";
            return await CallChatAsync(apiKey, model, prompt, cancellationToken);
        }

        private async Task<string> CallChatAsync(string apiKey, string model, string userPrompt, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var success = false;
            try
            {
                var request = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a research assistant. Be concise." },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.3
                };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
                httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                success = true;

                using var doc = JsonDocument.Parse(json);
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";
            }
            finally
            {
                sw.Stop();
                await _usageLogger.LogAsync("OpenAI", "chat/completions", success, (int)sw.ElapsedMilliseconds, success ? 0.01m : null);
            }
        }

        private static (string SummaryText, string? KeyFindings, string? Methodology) TryParseSummaryJson(string content)
        {
            try
            {
                var start = content.IndexOf('{');
                var end = content.LastIndexOf('}');
                if (start >= 0 && end > start)
                    content = content[start..(end + 1)];

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                return (
                    root.TryGetProperty("summaryText", out var s) ? s.GetString() ?? content : content,
                    root.TryGetProperty("keyFindings", out var k) ? k.GetString() : null,
                    root.TryGetProperty("methodology", out var m) ? m.GetString() : null
                );
            }
            catch
            {
                return (content, null, null);
            }
        }
    }
}
