namespace ResearchHub.API.Models.DTOs
{
    public class PaperDto
    {
        public int PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public int? PublicationYear { get; set; }
        public string? Abstract { get; set; }
        public string? Url { get; set; }
        public string? Source { get; set; }
        public int CitationCount { get; set; }
        public bool IsBookmarked { get; set; }
    }

    public class PaperSummaryDto
    {
        public int SummaryId { get; set; }
        public int PaperId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public string? KeyFindings { get; set; }
        public string? Methodology { get; set; }
        public bool GeneratedByAi { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SearchPapersResponse
    {
        public List<PaperDto> Results { get; set; } = new();
        public int Total { get; set; }
    }
}
