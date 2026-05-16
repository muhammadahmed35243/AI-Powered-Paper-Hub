using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.API.Data;
using ResearchHub.API.Extensions;
using ResearchHub.API.Helpers;
using ResearchHub.API.Models;
using ResearchHub.API.Models.DTOs;

namespace ResearchHub.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("groups")]
    public class GroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var group = new ResearchGroup
            {
                GroupName = request.GroupName,
                Description = request.Description,
                CreatedById = userId.Value,
                GroupCode = GroupCodeGenerator.Generate(),
                CreatedAt = DateTime.UtcNow
            };

            _context.ResearchGroups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = group.GroupId,
                UserId = userId.Value,
                JoinedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGroup), new { groupId = group.GroupId }, await MapGroupAsync(group.GroupId));
        }

        [HttpGet]
        public async Task<IActionResult> ListGroups()
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var groupIds = await _context.GroupMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.GroupId)
                .ToListAsync();

            var groups = new List<GroupDto>();
            foreach (var id in groupIds)
            {
                var dto = await MapGroupAsync(id);
                if (dto != null) groups.Add(dto);
            }

            return Ok(groups);
        }

        [HttpGet("{groupId:int}")]
        public async Task<IActionResult> GetGroup(int groupId)
        {
            if (!await IsMemberAsync(groupId)) return Forbid();
            var dto = await MapGroupAsync(groupId);
            return dto == null ? NotFound() : Ok(dto);
        }

        [HttpPut("{groupId:int}")]
        public async Task<IActionResult> UpdateGroup(int groupId, [FromBody] UpdateGroupRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var group = await _context.ResearchGroups.FindAsync(groupId);
            if (group == null) return NotFound();
            if (group.CreatedById != userId) return Forbid();

            if (!string.IsNullOrWhiteSpace(request.GroupName)) group.GroupName = request.GroupName;
            if (request.Description != null) group.Description = request.Description;
            await _context.SaveChangesAsync();

            return Ok(await MapGroupAsync(groupId));
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinGroup([FromBody] JoinGroupRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var group = await _context.ResearchGroups
                .FirstOrDefaultAsync(g => g.GroupCode == request.GroupCode);
            if (group == null) return NotFound(new { message = "Invalid group code." });

            var exists = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == group.GroupId && m.UserId == userId);
            if (exists) return BadRequest(new { message = "Already a member." });

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = group.GroupId,
                UserId = userId.Value,
                JoinedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(await MapGroupAsync(group.GroupId));
        }

        [HttpPost("{groupId:int}/members")]
        public async Task<IActionResult> AddMember(int groupId, [FromBody] JoinGroupRequest request)
        {
            return await JoinGroup(request);
        }

        [HttpGet("{groupId:int}/papers")]
        public async Task<IActionResult> GetGroupPapers(int groupId)
        {
            if (!await IsMemberAsync(groupId)) return Forbid();

            var papers = await _context.GroupPapers
                .Where(gp => gp.GroupId == groupId)
                .Include(gp => gp.Paper)
                .Include(gp => gp.AddedBy)
                .Select(gp => new
                {
                    gp.Paper.PaperId,
                    gp.Paper.Title,
                    gp.Paper.Authors,
                    gp.Paper.PublicationYear,
                    gp.Paper.Url,
                    gp.Tags,
                    gp.AddedAt,
                    AddedBy = gp.AddedBy.Name
                })
                .ToListAsync();

            return Ok(papers);
        }

        [HttpPost("{groupId:int}/papers/{paperId:int}")]
        public async Task<IActionResult> AddPaperToGroup(int groupId, int paperId, [FromBody] AddGroupPaperRequest? request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();
            if (!await IsMemberAsync(groupId)) return Forbid();

            var paper = await _context.Papers.FindAsync(paperId);
            if (paper == null) return NotFound(new { message = "Paper not found." });

            var exists = await _context.GroupPapers
                .AnyAsync(gp => gp.GroupId == groupId && gp.PaperId == paperId);
            if (exists) return BadRequest(new { message = "Paper already in group." });

            _context.GroupPapers.Add(new GroupPaper
            {
                GroupId = groupId,
                PaperId = paperId,
                AddedById = userId.Value,
                Tags = request?.Tags,
                AddedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<bool> IsMemberAsync(int groupId)
        {
            var userId = User.GetUserId();
            if (userId == null) return false;
            return await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
        }

        private async Task<GroupDto?> MapGroupAsync(int groupId)
        {
            var group = await _context.ResearchGroups
                .Include(g => g.Creator)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
            if (group == null) return null;

            return new GroupDto
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                Description = group.Description,
                GroupCode = group.GroupCode,
                CreatedById = group.CreatedById,
                CreatorName = group.Creator.Name,
                CreatedAt = group.CreatedAt,
                MemberCount = await _context.GroupMembers.CountAsync(m => m.GroupId == groupId),
                PaperCount = await _context.GroupPapers.CountAsync(p => p.GroupId == groupId)
            };
        }
    }
}
