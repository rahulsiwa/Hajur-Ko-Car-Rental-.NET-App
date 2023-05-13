using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalHajurKo.Migrations
{
    public partial class NoImageMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "DamageReport");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "DamageReport",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
