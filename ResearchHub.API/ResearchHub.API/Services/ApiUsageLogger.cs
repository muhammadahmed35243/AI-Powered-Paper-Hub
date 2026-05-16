using ResearchHub.API.Data;
using ResearchHub.API.Models;

namespace ResearchHub.API.Services
{
    public class ApiUsageLogger : IApiUsageLogger
    {
        private readonly ApplicationDbContext _context;

        public ApiUsageLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string apiName, string endpoint, bool success, int responseTimeMs, decimal? cost = null)
        {
            _context.ApiUsageLogs.Add(new ApiUsageLog
            {
                ApiName = apiName,
                Endpoint = endpoint,
                Success = success,
                ResponseTimeMs = responseTimeMs,
                Cost = cost,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}
