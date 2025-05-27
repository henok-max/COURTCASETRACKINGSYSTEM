using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtCaseTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDateTimeWithTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HearingDate",
                table: "Cases",
                newName: "HearingDateTime");

            migrationBuilder.AlterColumn<DateTime>(
                 name: "HearingDateTime",
                 table: "Cases",
                 type: "datetime2",
                 nullable: true,
                 oldClrType: typeof(DateTime),
                  oldType: "date",
                 oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "AppointmentDate",
                table: "Cases",
                newName: "AppointmentDateTime");

            migrationBuilder.AlterColumn<DateTime>(
                   name: "AppointmentDateTime",
                   table: "Cases",
                   type: "datetime2",
                   nullable: true,
                   oldClrType: typeof(DateTime),
                   oldType: "date",
                   oldNullable: true);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HearingDateTime",
                table: "Cases",
                newName: "HearingDate");

            migrationBuilder.RenameColumn(
                name: "AppointmentDateTime",
                table: "Cases",
                newName: "AppointmentDate");
        }
    }
}
