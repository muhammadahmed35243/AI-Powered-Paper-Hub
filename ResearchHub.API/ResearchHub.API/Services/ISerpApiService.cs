using ResearchHub.API.Models;

namespace ResearchHub.API.Services
{
    public interface ISerpApiService
    {
        Task<List<Paper>> SearchAsync(string query, string? field, int? yearFrom, int? yearTo, int maxResults, CancellationToken cancellationToken = default);
    }
}
