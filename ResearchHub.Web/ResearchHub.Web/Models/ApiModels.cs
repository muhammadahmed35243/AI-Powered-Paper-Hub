namespace ResearchHub.Web.Models
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

    public class SearchResponse
    {
        public List<PaperDto> Results { get; set; } = new();
        public int Total { get; set; }
    }

    public class PaperSummaryDto
    {
        public string SummaryText { get; set; } = string.Empty;
        public string? KeyFindings { get; set; }
        public string? Methodology { get; set; }
    }

    public class GroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? GroupCode { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int PaperCount { get; set; }
    }

    public class RecommendationDto
    {
        public int PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public int? PublicationYear { get; set; }
        public int CitationCount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
