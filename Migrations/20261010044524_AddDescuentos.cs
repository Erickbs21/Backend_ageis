using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiAegis.Migrations
{
    /// <inheritdoc />
    public partial class AddDescuentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "motivo_descuento",
                table: "ventas",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "descripcion", "nombre" },
                values: new object[] { 12, "Aplicar descuentos por producto o por venta", "AplicarDescuentos" });

            migrationBuilder.InsertData(
                table: "rol_permisos",
                columns: new[] { "id", "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { 23, 12, 1 },
                    { 24, 12, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "rol_permisos",
                keyColumn: "id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "rol_permisos",
                keyColumn: "id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DropColumn(
                name: "motivo_descuento",
                table: "ventas");
        }
    }
}
