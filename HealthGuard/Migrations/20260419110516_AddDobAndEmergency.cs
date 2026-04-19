using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddDobAndEmergency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "Patients",
                newName: "EmergencyContact");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Patients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DateOfBirth",
                table: "Patients",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "EmergencyContact",
                table: "Patients",
                newName: "Gender");

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "FullName",
                keyValue: null,
                column: "FullName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Patients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
