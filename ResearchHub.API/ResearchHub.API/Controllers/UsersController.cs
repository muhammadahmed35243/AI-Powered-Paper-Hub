using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.API.Data;
using ResearchHub.API.Extensions;
using ResearchHub.API.Models.DTOs;

namespace ResearchHub.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("user")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            if (request.ResearchInterests != null)
                user.ResearchInterests = request.ResearchInterests;

            await _context.SaveChangesAsync();
            return Ok(new { user.UserId, user.Name, user.Email, user.ResearchInterests });
        }

        [HttpGet("bookmarks")]
        public async Task<IActionResult> GetBookmarks()
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var bookmarks = await _context.UserBookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Paper)
                .OrderByDescending(b => b.SavedAt)
                .Select(b => new
                {
                    b.Paper.PaperId,
                    b.Paper.Title,
                    b.Paper.Authors,
                    b.Paper.PublicationYear,
                    b.Paper.Url,
                    b.SavedAt
                })
                .ToListAsync();

            return Ok(bookmarks);
        }
    }
}
