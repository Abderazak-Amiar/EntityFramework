using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    public partial class AddCompanyAddressAndPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // add new columns
            migrationBuilder.AddColumn<string>(
                name: "CompanyAddress",
                table: "Parameters",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPhone",
                table: "Parameters",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            // If you previously had a combined CompanyAddressPhone column, copy it into CompanyAddress
            // (This SQL will be a no-op if that column does not exist in the DB.)
            migrationBuilder.Sql(@"
                -- copy legacy data into CompanyAddress when column exists
                SELECT CASE WHEN EXISTS (SELECT 1 FROM pragma_table_info('Parameters') WHERE name = 'CompanyAddressPhone') THEN 1 ELSE 0 END;
            ");

            // copy only if legacy column exists (SQLite: safe to run even if column missing)
            migrationBuilder.Sql(@"
                UPDATE Parameters
                SET CompanyAddress = CompanyAddressPhone
                WHERE CompanyAddress IS NULL AND (SELECT COUNT(*) FROM pragma_table_info('Parameters') WHERE name = 'CompanyAddressPhone') = 1;
            ");

            // optionally drop the legacy column (only do this when you no longer need it)
            migrationBuilder.Sql(@"
                -- drop column is not supported directly by SQLite; you would need to rebuild table.
                -- If you want to drop CompanyAddressPhone later, create a separate migration that rebuilds the table.
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // In Down we remove the two columns
            migrationBuilder.DropColumn(
                name: "CompanyAddress",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "CompanyPhone",
                table: "Parameters");
        }
    }
}
