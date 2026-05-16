using System;

namespace ResearchHub.API.Models
{
    public class UserInterest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public int InterestLevel { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
