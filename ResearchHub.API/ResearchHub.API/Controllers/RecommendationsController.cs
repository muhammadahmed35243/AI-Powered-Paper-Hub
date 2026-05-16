using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.API.Data;
using ResearchHub.API.Extensions;
using ResearchHub.API.Models.DTOs;
using ResearchHub.API.Services;

namespace ResearchHub.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("recommendations")]
    public class RecommendationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOpenAiService _openAi;

        public RecommendationsController(ApplicationDbContext context, IOpenAiService openAi)
        {
            _context = context;
            _openAi = openAi;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendations([FromQuery] int limit = 10)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var topTopics = await _context.UserInterests
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.InterestLevel)
                .Select(i => i.Topic)
                .Take(3)
                .ToListAsync();

            var bookmarkedIds = await _context.UserBookmarks
                .Where(b => b.UserId == userId)
                .Select(b => b.PaperId)
                .ToListAsync();

            IQueryable<Models.Paper> query = _context.Papers;
            if (topTopics.Count > 0)
            {
                query = query.Where(p => topTopics.Any(t =>
                    p.Title.Contains(t) ||
                    (p.Abstract != null && p.Abstract.Contains(t))));
            }

            var papers = await query
                .Where(p => !bookmarkedIds.Contains(p.PaperId))
                .OrderByDescending(p => p.CitationCount)
                .ThenByDescending(p => p.FetchedAt)
                .Take(limit)
                .ToListAsync();

            var topic = topTopics.FirstOrDefault() ?? "your research interests";
            var results = new List<object>();
            foreach (var paper in papers)
            {
                string reason;
                try
                {
                    reason = await _openAi.ExplainRecommendationAsync(topic, paper);
                }
                catch
                {
                    reason = $"Related to {topic} based on your activity.";
                }

                results.Add(new
                {
                    paper.PaperId,
                    paper.Title,
                    paper.Authors,
                    paper.PublicationYear,
                    paper.CitationCount,
                    Reason = reason
                });
            }

            return Ok(results);
        }

        [HttpGet("reasons/{paperId:int}")]
        public async Task<IActionResult> GetReason(int paperId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var paper = await _context.Papers.FindAsync(paperId);
            if (paper == null) return NotFound();

            var topic = await _context.UserInterests
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.InterestLevel)
                .Select(i => i.Topic)
                .FirstOrDefaultAsync() ?? "your research";

            try
            {
                var reason = await _openAi.ExplainRecommendationAsync(topic, paper);
                return Ok(new { paperId, reason });
            }
            catch
            {
                return Ok(new { paperId, reason = $"Matches your interest in {topic}." });
            }
        }
    }
}
