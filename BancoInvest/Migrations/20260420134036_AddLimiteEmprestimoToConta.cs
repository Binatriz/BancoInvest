using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancoInvest.Migrations
{
    /// <inheritdoc />
    public partial class AddLimiteEmprestimoToConta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LimiteEmprestimo",
                table: "Contas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LimiteEmprestimo",
                table: "Contas");
        }
    }
}
