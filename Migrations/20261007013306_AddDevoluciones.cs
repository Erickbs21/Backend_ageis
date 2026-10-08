using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiAegis.Migrations
{
    /// <inheritdoc />
    public partial class AddDevoluciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "monto_devuelto",
                table: "devoluciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "descripcion", "nombre" },
                values: new object[] { 11, "Registrar devoluciones totales y parciales de ventas", "GestionDevoluciones" });

            migrationBuilder.InsertData(
                table: "rol_permisos",
                columns: new[] { "id", "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { 21, 11, 1 },
                    { 22, 11, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "rol_permisos",
                keyColumn: "id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "rol_permisos",
                keyColumn: "id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "monto_devuelto",
                table: "devoluciones");
        }
    }
}
