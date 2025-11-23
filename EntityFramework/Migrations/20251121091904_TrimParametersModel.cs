using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class TrimParametersModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CultureName",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "DefaultWeightUnit",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "PrinterName",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "UseSilentPrinting",
                table: "Parameters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CultureName",
                table: "Parameters",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultWeightUnit",
                table: "Parameters",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PrinterName",
                table: "Parameters",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseSilentPrinting",
                table: "Parameters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
