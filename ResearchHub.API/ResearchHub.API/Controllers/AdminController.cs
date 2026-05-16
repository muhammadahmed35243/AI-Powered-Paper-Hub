using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.API.Data;

namespace ResearchHub.API.Controllers
{
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.IsAdmin,
                    u.CreatedAt,
                    GroupCount = u.GroupMemberships.Count,
                    BookmarkCount = u.Bookmarks.Count
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.ResearchGroups
                .Include(g => g.Creator)
                .Select(g => new
                {
                    g.GroupId,
                    g.GroupName,
                    g.GroupCode,
                    Creator = g.Creator.Name,
                    g.CreatedAt,
                    MemberCount = g.Members.Count,
                    PaperCount = g.Papers.Count
                })
                .ToListAsync();
            return Ok(groups);
        }

        [HttpGet("analytics/usage")]
        public async Task<IActionResult> GetApiUsage([FromQuery] int days = 30)
        {
            var since = DateTime.UtcNow.AddDays(-days);
            var logs = await _context.ApiUsageLogs
                .Where(l => l.Timestamp >= since)
                .GroupBy(l => l.ApiName)
                .Select(g => new
                {
                    ApiName = g.Key,
                    TotalCalls = g.Count(),
                    SuccessRate = g.Count(x => x.Success) * 100.0 / g.Count(),
                    AvgResponseMs = g.Average(x => x.ResponseTimeMs),
                    TotalCost = g.Sum(x => x.Cost ?? 0)
                })
                .ToListAsync();
            return Ok(logs);
        }

        [HttpGet("analytics/trending")]
        public async Task<IActionResult> GetTrending()
        {
            var topics = await _context.UserInterests
                .GroupBy(i => i.Topic)
                .Select(g => new { Topic = g.Key, Score = g.Sum(i => i.InterestLevel), Searches = g.Count() })
                .OrderByDescending(x => x.Score)
                .Take(15)
                .ToListAsync();

            var popularPapers = await _context.UserBookmarks
                .GroupBy(b => b.PaperId)
                .Select(g => new { PaperId = g.Key, Saves = g.Count() })
                .OrderByDescending(x => x.Saves)
                .Take(10)
                .Join(_context.Papers, x => x.PaperId, p => p.PaperId, (x, p) => new { p.Title, x.Saves })
                .ToListAsync();

            return Ok(new { topics, popularPapers });
        }
    }
}
