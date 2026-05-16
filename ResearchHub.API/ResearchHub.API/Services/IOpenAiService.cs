using ResearchHub.API.Models;

namespace ResearchHub.API.Services
{
    public interface IOpenAiService
    {
        Task<PaperSummary> GenerateSummaryAsync(Paper paper, CancellationToken cancellationToken = default);
        Task<string> ExplainRecommendationAsync(string topic, Paper paper, CancellationToken cancellationToken = default);
    }
}
