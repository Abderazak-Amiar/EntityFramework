using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PrinterName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DefaultUnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    DefaultWeightUnit = table.Column<decimal>(type: "TEXT", nullable: false),
                    UseSilentPrinting = table.Column<bool>(type: "INTEGER", nullable: false),
                    CultureName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parameters");
        }
    }
}
