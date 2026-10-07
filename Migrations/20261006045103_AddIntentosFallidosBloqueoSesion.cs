using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiAegis.Migrations
{
    /// <inheritdoc />
    public partial class AddIntentosFallidosBloqueoSesion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "bloqueado_hasta",
                table: "usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "intentos_fallidos",
                table: "usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "bloqueado_hasta", "intentos_fallidos", "password_hash" },
                values: new object[] { null, 0, "$2a$11$xNL0iKtHeDUUkul6z0EyluS.k8sbMY/m.Zyxi5vt27HLs0X6rnvCi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bloqueado_hasta",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "intentos_fallidos",
                table: "usuarios");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$.VtsimgEsCaKe3sy1AG6HuF9rRe1nm0JepyT4CJIMtkmMf662rJBC");
        }
    }
}
