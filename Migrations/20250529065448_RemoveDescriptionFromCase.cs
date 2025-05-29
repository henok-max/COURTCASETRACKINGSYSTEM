using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtCaseTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDescriptionFromCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Cases");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
