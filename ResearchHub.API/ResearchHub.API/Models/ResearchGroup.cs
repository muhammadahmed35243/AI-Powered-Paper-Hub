using System;
using System.Collections.Generic;

namespace ResearchHub.API.Models
{
    public class ResearchGroup
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CreatedById { get; set; }
        public string? GroupCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Creator { get; set; } = null!;
        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public ICollection<GroupPaper> Papers { get; set; } = new List<GroupPaper>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
