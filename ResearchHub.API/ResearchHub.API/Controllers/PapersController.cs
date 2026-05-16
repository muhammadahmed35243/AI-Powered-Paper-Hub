using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.API.Data;
using ResearchHub.API.Extensions;
using ResearchHub.API.Models;
using ResearchHub.API.Models.DTOs;
using ResearchHub.API.Services;

namespace ResearchHub.API.Controllers
{
    [ApiController]
    [Route("papers")]
    public class PapersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISerpApiService _serpApi;
        private readonly IOpenAiService _openAi;
        private readonly IOpenAlexService _openAlex;
        private readonly IConfiguration _configuration;

        public PapersController(
            ApplicationDbContext context,
            ISerpApiService serpApi,
            IOpenAiService openAi,
            IOpenAlexService openAlex,
            IConfiguration configuration)
        {
            _context = context;
            _serpApi = serpApi;
            _openAi = openAi;
            _openAlex = openAlex;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] string? field = null,
            [FromQuery] int? year_from = null,
            [FromQuery] int? year_to = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Query is required." });

            var userId = User.GetUserId();
            var maxResults = _configuration.GetValue("SerpApi:MaxResults", 10);

            List<Paper> results;
            try
            {
                results = await _serpApi.SearchAsync(query, field, year_from, year_to, maxResults);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }

            var saved = new List<Paper>();
            foreach (var paper in results)
            {
                var existing = await _context.Papers
                    .FirstOrDefaultAsync(p => p.Url != null && p.Url == paper.Url);
                if (existing != null)
                {
                    saved.Add(existing);
                    continue;
                }

                await _openAlex.EnrichPaperAsync(paper);
                _context.Papers.Add(paper);
                saved.Add(paper);
            }
            await _context.SaveChangesAsync();

            if (userId.HasValue)
            {
                await TrackInterestAsync(userId.Value, query);
            }

            var bookmarkedIds = userId.HasValue
                ? await _context.UserBookmarks
                    .Where(b => b.UserId == userId)
                    .Select(b => b.PaperId)
                    .ToListAsync()
                : new List<int>();

            return Ok(new SearchPapersResponse
            {
                Total = saved.Count,
                Results = saved.Select(p => ToDto(p, bookmarkedIds.Contains(p.PaperId))).ToList()
            });
        }

        [HttpGet("{paperId:int}")]
        public async Task<IActionResult> GetPaper(int paperId)
        {
            var paper = await _context.Papers.FindAsync(paperId);
            if (paper == null) return NotFound();

            var userId = User.GetUserId();
            var isBookmarked = userId.HasValue && await _context.UserBookmarks
                .AnyAsync(b => b.UserId == userId && b.PaperId == paperId);

            return Ok(ToDto(paper, isBookmarked));
        }

        [Authorize]
        [HttpGet("{paperId:int}/summary")]
        public async Task<IActionResult> GetSummary(int paperId)
        {
            var paper = await _context.Papers.FindAsync(paperId);
            if (paper == null) return NotFound();

            var cached = await _context.PaperSummaries
                .Where(s => s.PaperId == paperId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (cached != null)
                return Ok(ToSummaryDto(cached));

            try
            {
                var summary = await _openAi.GenerateSummaryAsync(paper);
                summary.PaperId = paperId;
                _context.PaperSummaries.Add(summary);
                await _context.SaveChangesAsync();
                return Ok(ToSummaryDto(summary));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{paperId:int}/bookmark")]
        public async Task<IActionResult> Bookmark(int paperId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var paper = await _context.Papers.FindAsync(paperId);
            if (paper == null) return NotFound();

            var exists = await _context.UserBookmarks
                .AnyAsync(b => b.UserId == userId && b.PaperId == paperId);
            if (exists) return Ok(new { message = "Already bookmarked." });

            _context.UserBookmarks.Add(new UserBookmark
            {
                UserId = userId.Value,
                PaperId = paperId,
                SavedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bookmarked." });
        }

        private async Task TrackInterestAsync(int userId, string topic)
        {
            var existing = await _context.UserInterests
                .FirstOrDefaultAsync(i => i.UserId == userId && i.Topic == topic);
            if (existing != null)
            {
                existing.InterestLevel = Math.Min(10, existing.InterestLevel + 1);
            }
            else
            {
                _context.UserInterests.Add(new UserInterest
                {
                    UserId = userId,
                    Topic = topic.Length > 200 ? topic[..200] : topic,
                    InterestLevel = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();
        }

        private static PaperDto ToDto(Paper p, bool isBookmarked) => new()
        {
            PaperId = p.PaperId,
            Title = p.Title,
            Authors = p.Authors,
            PublicationYear = p.PublicationYear,
            Abstract = p.Abstract,
            Url = p.Url,
            Source = p.Source,
            CitationCount = p.CitationCount,
            IsBookmarked = isBookmarked
        };

        private static PaperSummaryDto ToSummaryDto(PaperSummary s) => new()
        {
            SummaryId = s.SummaryId,
            PaperId = s.PaperId,
            SummaryText = s.SummaryText,
            KeyFindings = s.KeyFindings,
            Methodology = s.Methodology,
            GeneratedByAi = s.GeneratedByAi,
            CreatedAt = s.CreatedAt
        };
    }
}
