using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiAegis.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioToCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "usuario_id",
                table: "cajas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cajas_usuario_id",
                table: "cajas",
                column: "usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_cajas_usuarios_usuario_id",
                table: "cajas",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cajas_usuarios_usuario_id",
                table: "cajas");

            migrationBuilder.DropIndex(
                name: "IX_cajas_usuario_id",
                table: "cajas");

            migrationBuilder.DropColumn(
                name: "usuario_id",
                table: "cajas");
        }
    }
}
