using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApp.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntityBeneficiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "InterestRate",
                table: "Loan",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Loan",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Loan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Loan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Beneficiarys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BeneficiaryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiarys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Commerce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commerce", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Beneficiarys");

            migrationBuilder.DropTable(
                name: "Commerce");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Loan");

            migrationBuilder.AlterColumn<decimal>(
                name: "InterestRate",
                table: "Loan",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
