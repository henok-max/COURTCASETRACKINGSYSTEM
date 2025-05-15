using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtCaseTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseDetailsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DecreeHistory",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JudgmentHistory",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plea",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeHistory",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "JudgmentHistory",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "Plea",
                table: "Cases");
        }
    }
}
