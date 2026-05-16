using System;
using System.Collections.Generic;

namespace ResearchHub.API.Models
{
    public class Paper
    {
        public int PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public int? PublicationYear { get; set; }
        public string? Abstract { get; set; }
        public string? Url { get; set; }
        public string? Source { get; set; }
        public int CitationCount { get; set; } = 0;
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<PaperSummary> Summaries { get; set; } = new List<PaperSummary>();
        public ICollection<GroupPaper> GroupPapers { get; set; } = new List<GroupPaper>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
