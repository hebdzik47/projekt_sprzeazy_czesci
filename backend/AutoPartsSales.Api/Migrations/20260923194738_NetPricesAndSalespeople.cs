using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoPartsSales.Api.Migrations
{
    /// <inheritdoc />
    public partial class NetPricesAndSalespeople : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OriginalGrossPrice",
                table: "Sales",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalespersonId",
                table: "Sales",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Salespeople",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salespeople", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Salespeople",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Krystian" },
                    { 2, "Adam" },
                    { 3, "Damian" },
                    { 4, "Paweł" },
                    { 5, "sprzedaż na miejscu" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SalespersonId",
                table: "Sales",
                column: "SalespersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Salespeople_SalespersonId",
                table: "Sales",
                column: "SalespersonId",
                principalTable: "Salespeople",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            // Convert legacy gross amounts once, retaining the exact original amount.
            migrationBuilder.Sql("""
                UPDATE "Sales" SET "OriginalGrossPrice" = "Price", "Price" = ROUND("Price" / 1.23, 2);
                UPDATE "Cars" SET "Make" = 'vw' WHERE LOWER(TRIM("Make")) IN ('volkswagen', 'vw');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Sales" SET "Price" = COALESCE("OriginalGrossPrice", ROUND("Price" * 1.23, 2));
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Salespeople_SalespersonId",
                table: "Sales");

            migrationBuilder.DropTable(
                name: "Salespeople");

            migrationBuilder.DropIndex(
                name: "IX_Sales_SalespersonId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "OriginalGrossPrice",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "SalespersonId",
                table: "Sales");
        }
    }
}
