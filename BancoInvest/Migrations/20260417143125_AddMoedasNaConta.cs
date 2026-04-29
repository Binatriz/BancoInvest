using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancoInvest.Migrations
{
    /// <inheritdoc />
    public partial class AddMoedasNaConta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Saldo",
                table: "Contas",
                newName: "SaldoUSD");

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoBRL",
                table: "Contas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoEUR",
                table: "Contas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoGBP",
                table: "Contas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoJPY",
                table: "Contas",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaldoBRL",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "SaldoEUR",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "SaldoGBP",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "SaldoJPY",
                table: "Contas");

            migrationBuilder.RenameColumn(
                name: "SaldoUSD",
                table: "Contas",
                newName: "Saldo");
        }
    }
}
