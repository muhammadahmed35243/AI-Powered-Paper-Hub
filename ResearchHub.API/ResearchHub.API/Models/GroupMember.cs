using System;

namespace ResearchHub.API.Models
{
    public class GroupMember
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ResearchGroup Group { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
