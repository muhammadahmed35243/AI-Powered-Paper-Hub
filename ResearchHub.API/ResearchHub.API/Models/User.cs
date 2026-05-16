using System;
using System.Collections.Generic;

namespace ResearchHub.API.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? GoogleId { get; set; }
        public string? ResearchInterests { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ResearchGroup> CreatedGroups { get; set; } = new List<ResearchGroup>();
        public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
        public ICollection<GroupPaper> AddedPapers { get; set; } = new List<GroupPaper>();
        public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<UserBookmark> Bookmarks { get; set; } = new List<UserBookmark>();
    }
}
