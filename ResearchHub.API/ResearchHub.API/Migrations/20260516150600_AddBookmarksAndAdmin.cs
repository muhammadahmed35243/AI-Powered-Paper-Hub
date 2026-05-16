using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBookmarksAndAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "GroupPapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserBookmarks",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBookmarks", x => new { x.UserId, x.PaperId });
                    table.ForeignKey(
                        name: "FK_UserBookmarks_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "PaperId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBookmarks_PaperId",
                table: "UserBookmarks",
                column: "PaperId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBookmarks");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "GroupPapers");
        }
    }
}
