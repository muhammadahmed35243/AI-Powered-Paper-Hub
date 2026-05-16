using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchHub.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiUsageLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiUsageLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "Papers",
                columns: table => new
                {
                    PaperId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Authors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicationYear = table.Column<int>(type: "int", nullable: true),
                    Abstract = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CitationCount = table.Column<int>(type: "int", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Papers", x => x.PaperId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoogleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResearchInterests = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "PaperSummaries",
                columns: table => new
                {
                    SummaryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    SummaryText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeyFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Methodology = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedByAi = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperSummaries", x => x.SummaryId);
                    table.ForeignKey(
                        name: "FK_PaperSummaries_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "PaperId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchGroups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    GroupCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchGroups", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_ResearchGroups_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterestLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInterests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => new { x.GroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GroupMembers_ResearchGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ResearchGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupPapers",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    AddedById = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPapers", x => new { x.GroupId, x.PaperId });
                    table.ForeignKey(
                        name: "FK_GroupPapers_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "PaperId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPapers_ResearchGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ResearchGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPapers_Users_AddedById",
                        column: x => x.AddedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    NoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    NoteText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.NoteId);
                    table.ForeignKey(
                        name: "FK_Notes_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "PaperId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notes_ResearchGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ResearchGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPapers_AddedById",
                table: "GroupPapers",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPapers_PaperId",
                table: "GroupPapers",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_GroupId",
                table: "Notes",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_PaperId",
                table: "Notes",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UserId",
                table: "Notes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperSummaries_PaperId",
                table: "PaperSummaries",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchGroups_CreatedById",
                table: "ResearchGroups",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UserId",
                table: "UserInterests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiUsageLogs");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "GroupPapers");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "PaperSummaries");

            migrationBuilder.DropTable(
                name: "UserInterests");

            migrationBuilder.DropTable(
                name: "ResearchGroups");

            migrationBuilder.DropTable(
                name: "Papers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
