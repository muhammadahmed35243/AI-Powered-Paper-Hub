namespace ResearchHub.API.Models.DTOs
{
    public class CreateGroupRequest
    {
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateGroupRequest
    {
        public string? GroupName { get; set; }
        public string? Description { get; set; }
    }

    public class JoinGroupRequest
    {
        public string GroupCode { get; set; } = string.Empty;
    }

    public class GroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? GroupCode { get; set; }
        public int CreatedById { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public int PaperCount { get; set; }
    }

    public class AddGroupPaperRequest
    {
        public int PaperId { get; set; }
        public string? Tags { get; set; }
    }

    public class CreateNoteRequest
    {
        public int PaperId { get; set; }
        public int? GroupId { get; set; }
        public string NoteText { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string? ResearchInterests { get; set; }
    }
}
