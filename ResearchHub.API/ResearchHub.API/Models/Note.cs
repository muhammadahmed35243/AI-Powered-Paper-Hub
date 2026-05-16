using System;

namespace ResearchHub.API.Models
{
    public class Note
    {
        public int NoteId { get; set; }
        public int PaperId { get; set; }
        public int UserId { get; set; }
        public int? GroupId { get; set; }
        public string NoteText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Paper Paper { get; set; } = null!;
        public User User { get; set; } = null!;
        public ResearchGroup? Group { get; set; }
    }
}
