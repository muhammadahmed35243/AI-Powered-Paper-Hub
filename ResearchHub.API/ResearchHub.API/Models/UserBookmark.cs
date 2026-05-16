using System;

namespace ResearchHub.API.Models
{
    public class UserBookmark
    {
        public int UserId { get; set; }
        public int PaperId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
    }
}
