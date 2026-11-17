using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsSales.Api.Migrations
{
    /// <inheritdoc />
    public partial class LinkEmployeeAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalespersonId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SalespersonId",
                table: "AspNetUsers",
                column: "SalespersonId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Salespeople_SalespersonId",
                table: "AspNetUsers",
                column: "SalespersonId",
                principalTable: "Salespeople",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Salespeople_SalespersonId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SalespersonId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SalespersonId",
                table: "AspNetUsers");
        }
    }
}
