using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiAegis.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditosAbonos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "creditos_abonos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    venta_id = table.Column<int>(type: "int", nullable: false),
                    metodo_pago_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    caja_apertura_id = table.Column<int>(type: "int", nullable: true),
                    monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    observacion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_abono = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creditos_abonos", x => x.id);
                    table.ForeignKey(
                        name: "FK_creditos_abonos_caja_aperturas_caja_apertura_id",
                        column: x => x.caja_apertura_id,
                        principalTable: "caja_aperturas",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_creditos_abonos_metodos_pago_metodo_pago_id",
                        column: x => x.metodo_pago_id,
                        principalTable: "metodos_pago",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_creditos_abonos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_creditos_abonos_ventas_venta_id",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "descripcion", "nombre" },
                values: new object[] { 13, "Cobrar abonos y gestionar la cartera de crédito de clientes", "GestionCredito" });

            migrationBuilder.InsertData(
                table: "rol_permisos",
                columns: new[] { "id", "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { 25, 13, 1 },
                    { 26, 13, 2 },
                    { 27, 13, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_creditos_abonos_caja_apertura_id",
                table: "creditos_abonos",
                column: "caja_apertura_id");

            migrationBuilder.CreateIndex(
                name: "IX_creditos_abonos_metodo_pago_id",
                table: "creditos_abonos",
                column: "metodo_pago_id");

            migrationBuilder.CreateIndex(
                name: "IX_creditos_abonos_usuario_id",
                table: "creditos_abonos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_creditos_abonos_venta_id",
                table: "creditos_abonos",
                column: "venta_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "creditos_abonos");

            migrationBuilder.DeleteData(
                table: "rol_permisos",
                keyColumn: "id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "rol_permisos",
                keyColumn: "id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "rol_permisos",
                keyColumn: "id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: 13);
        }
    }
}
