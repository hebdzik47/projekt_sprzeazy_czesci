using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsSales.Api.Migrations
{
    /// <inheritdoc />
    public partial class SaleQuantities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Sales",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sales_Quantity",
                table: "Sales",
                sql: "\"Quantity\" BETWEEN 1 AND 10000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Sales_Quantity",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Sales");
        }
    }
}
