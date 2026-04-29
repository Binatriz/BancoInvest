using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancoInvest.Migrations
{
    /// <inheritdoc />
    public partial class AddCambioToTransacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CambioId",
                table: "Transacoes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_CambioId",
                table: "Transacoes",
                column: "CambioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Cambios_CambioId",
                table: "Transacoes",
                column: "CambioId",
                principalTable: "Cambios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Cambios_CambioId",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Transacoes_CambioId",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "CambioId",
                table: "Transacoes");
        }
    }
}
