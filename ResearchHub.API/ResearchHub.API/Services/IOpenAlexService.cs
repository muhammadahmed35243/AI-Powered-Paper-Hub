using ResearchHub.API.Models;

namespace ResearchHub.API.Services
{
    public interface IOpenAlexService
    {
        Task EnrichPaperAsync(Paper paper, CancellationToken cancellationToken = default);
    }
}
