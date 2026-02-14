using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _10_Code_first.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Instructors",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Instructors",
                type: "VARCHAR(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode",
                table: "Instructors",
                type: "VARCHAR(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Instructors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Address_City", "Address_Street", "Address_ZipCode" },
                values: new object[] { "Cairo", "123 Main St", "11511" });

            migrationBuilder.UpdateData(
                table: "Instructors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Address_City", "Address_Street", "Address_ZipCode" },
                values: new object[] { "Alexandria", "456 Elm Ave", "21500" });

            migrationBuilder.UpdateData(
                table: "Instructors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Address_City", "Address_Street", "Address_ZipCode" },
                values: new object[] { "Cairo", "789 Oak Blvd", "11765" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "Address_ZipCode",
                table: "Instructors");
        }
    }
}
