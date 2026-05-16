using System;

namespace ResearchHub.API.Models
{
    public class PaperSummary
    {
        public int SummaryId { get; set; }
        public int PaperId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public string? KeyFindings { get; set; }
        public string? Methodology { get; set; }
        public bool GeneratedByAi { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Paper Paper { get; set; } = null!;
    }
}
