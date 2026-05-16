using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.API.Data;
using ResearchHub.API.Extensions;
using ResearchHub.API.Models;
using ResearchHub.API.Models.DTOs;

namespace ResearchHub.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("notes")]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int? paperId, [FromQuery] int? groupId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var query = _context.Notes
                .Include(n => n.User)
                .Include(n => n.Paper)
                .AsQueryable();

            if (paperId.HasValue) query = query.Where(n => n.PaperId == paperId);
            if (groupId.HasValue)
            {
                if (!await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId))
                    return Forbid();
                query = query.Where(n => n.GroupId == groupId);
            }
            else
            {
                query = query.Where(n => n.UserId == userId || (n.GroupId != null &&
                    _context.GroupMembers.Any(m => m.GroupId == n.GroupId && m.UserId == userId)));
            }

            var notes = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.NoteId,
                    n.PaperId,
                    PaperTitle = n.Paper.Title,
                    n.GroupId,
                    n.NoteText,
                    n.CreatedAt,
                    Author = n.User.Name,
                    IsOwn = n.UserId == userId
                })
                .ToListAsync();

            return Ok(notes);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNoteRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            if (request.GroupId.HasValue &&
                !await _context.GroupMembers.AnyAsync(m => m.GroupId == request.GroupId && m.UserId == userId))
                return Forbid();

            var note = new Note
            {
                PaperId = request.PaperId,
                UserId = userId.Value,
                GroupId = request.GroupId,
                NoteText = request.NoteText,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(List), new { paperId = note.PaperId }, new { note.NoteId, note.NoteText });
        }

        [HttpDelete("{noteId:int}")]
        public async Task<IActionResult> Delete(int noteId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var note = await _context.Notes.FindAsync(noteId);
            if (note == null) return NotFound();
            if (note.UserId != userId) return Forbid();

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
