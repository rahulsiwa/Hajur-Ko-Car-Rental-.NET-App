using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalHajurKo.Migrations
{
    public partial class PriceMIgration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "Car",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "Price",
                table: "Car",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Car");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Car",
                newName: "status");
        }
    }
}
