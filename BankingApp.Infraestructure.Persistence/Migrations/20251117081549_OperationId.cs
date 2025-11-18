using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApp.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OperationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OperationId",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "Transaction");
        }
    }
}
