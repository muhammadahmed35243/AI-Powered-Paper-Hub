using System;

namespace ResearchHub.API.Models
{
    public class ApiUsageLog
    {
        public int LogId { get; set; }
        public string ApiName { get; set; } = string.Empty; // SerpAPI, OpenAI, OpenAlex
        public string Endpoint { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public int ResponseTimeMs { get; set; }
        public decimal? Cost { get; set; }
    }
}
