using Microsoft.EntityFrameworkCore;
using ResearchHub.API.Models;

namespace ResearchHub.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ResearchGroup> ResearchGroups { get; set; } = null!;
        public DbSet<GroupMember> GroupMembers { get; set; } = null!;
        public DbSet<Paper> Papers { get; set; } = null!;
        public DbSet<PaperSummary> PaperSummaries { get; set; } = null!;
        public DbSet<GroupPaper> GroupPapers { get; set; } = null!;
        public DbSet<UserInterest> UserInterests { get; set; } = null!;
        public DbSet<Note> Notes { get; set; } = null!;
        public DbSet<ApiUsageLog> ApiUsageLogs { get; set; } = null!;
        public DbSet<UserBookmark> UserBookmarks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicit Primary Keys
            modelBuilder.Entity<User>().HasKey(e => e.UserId);
            modelBuilder.Entity<ResearchGroup>().HasKey(e => e.GroupId);
            modelBuilder.Entity<Paper>().HasKey(e => e.PaperId);
            modelBuilder.Entity<PaperSummary>().HasKey(e => e.SummaryId);
            modelBuilder.Entity<UserInterest>().HasKey(e => e.Id);
            modelBuilder.Entity<Note>().HasKey(e => e.NoteId);
            modelBuilder.Entity<ApiUsageLog>().HasKey(e => e.LogId);
            // Composite Keys
            modelBuilder.Entity<GroupMember>()
                .HasKey(gm => new { gm.GroupId, gm.UserId });

            modelBuilder.Entity<GroupPaper>()
                .HasKey(gp => new { gp.GroupId, gp.PaperId });

            modelBuilder.Entity<UserBookmark>()
                .HasKey(ub => new { ub.UserId, ub.PaperId });

            modelBuilder.Entity<UserBookmark>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBookmark>()
                .HasOne(ub => ub.Paper)
                .WithMany()
                .HasForeignKey(ub => ub.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - ResearchGroup (Creator)
            modelBuilder.Entity<ResearchGroup>()
                .HasOne(rg => rg.Creator)
                .WithMany(u => u.CreatedGroups)
                .HasForeignKey(rg => rg.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupMember Relationships
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(rg => rg.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupPaper Relationships
            modelBuilder.Entity<GroupPaper>()
                .HasOne(gp => gp.Group)
                .WithMany(rg => rg.Papers)
                .HasForeignKey(gp => gp.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupPaper>()
                .HasOne(gp => gp.Paper)
                .WithMany(p => p.GroupPapers)
                .HasForeignKey(gp => gp.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupPaper>()
                .HasOne(gp => gp.AddedBy)
                .WithMany(u => u.AddedPapers)
                .HasForeignKey(gp => gp.AddedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Note Relationships
            modelBuilder.Entity<Note>()
                .HasOne(n => n.Paper)
                .WithMany(p => p.Notes)
                .HasForeignKey(n => n.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Group)
                .WithMany(rg => rg.Notes)
                .HasForeignKey(n => n.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // PaperSummary Relationships
            modelBuilder.Entity<PaperSummary>()
                .HasOne(ps => ps.Paper)
                .WithMany(p => p.Summaries)
                .HasForeignKey(ps => ps.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserInterest Relationships
            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.Interests)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // ApiUsageLog Cost Precision
            modelBuilder.Entity<ApiUsageLog>()
                .Property(a => a.Cost)
                .HasPrecision(18, 4);
        }
    }
}
