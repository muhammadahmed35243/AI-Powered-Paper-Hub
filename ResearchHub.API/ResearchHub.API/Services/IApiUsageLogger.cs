namespace ResearchHub.API.Services
{
    public interface IApiUsageLogger
    {
        Task LogAsync(string apiName, string endpoint, bool success, int responseTimeMs, decimal? cost = null);
    }
}
