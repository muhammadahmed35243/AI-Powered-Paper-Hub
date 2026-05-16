using System;

namespace ResearchHub.API.Models
{
    public class GroupPaper
    {
        public int GroupId { get; set; }
        public int PaperId { get; set; }
        public int AddedById { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public string? Tags { get; set; }

        // Navigation properties
        public ResearchGroup Group { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
        public User AddedBy { get; set; } = null!;
    }
}
