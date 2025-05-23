using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtCaseTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixPhoneNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefendantPhone",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DefendantSmsOptIn",
                table: "Cases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlaintiffPhone",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PlaintiffSmsOptIn",
                table: "Cases",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefendantPhone",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "DefendantSmsOptIn",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "PlaintiffPhone",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "PlaintiffSmsOptIn",
                table: "Cases");
        }
    }
}
