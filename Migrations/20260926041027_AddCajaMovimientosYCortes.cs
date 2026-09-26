using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiAegis.Migrations
{
    /// <inheritdoc />
    public partial class AddCajaMovimientosYCortes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "caja_apertura_id",
                table: "ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "notas",
                table: "caja_cierres",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "billetes_q10",
                table: "caja_cierres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "billetes_q100",
                table: "caja_cierres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "billetes_q20",
                table: "caja_cierres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "billetes_q200",
                table: "caja_cierres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "billetes_q5",
                table: "caja_cierres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "billetes_q50",
                table: "caja_cierres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "caja_apertura_id",
                table: "caja_cierres",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cantidad_ventas",
                table: "caja_cierres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "devoluciones",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "diferencia",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "efectivo_contado",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "efectivo_esperado",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "entradas_efectivo",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "estado_corte",
                table: "caja_cierres",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "fondo_inicial",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "monedas",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "salidas_efectivo",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "tipo_corte",
                table: "caja_cierres",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "total_conteo",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_ventas",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ventas_anuladas",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ventas_cheque",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ventas_credito",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ventas_efectivo",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ventas_mixto",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ventas_tarjeta",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ventas_transferencia",
                table: "caja_cierres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "asignado_por",
                table: "caja_aperturas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observaciones",
                table: "caja_aperturas",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "caja_movimientos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    caja_apertura_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    motivo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observacion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_movimientos", x => x.id);
                    table.ForeignKey(
                        name: "FK_caja_movimientos_caja_aperturas_caja_apertura_id",
                        column: x => x.caja_apertura_id,
                        principalTable: "caja_aperturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_caja_movimientos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$1VGFXpJVcst493X3Ijvfd.GFTYXRNKU/GkLOxSkoauANMvhtM3blC");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_caja_apertura_id",
                table: "ventas",
                column: "caja_apertura_id");

            migrationBuilder.CreateIndex(
                name: "IX_caja_cierres_caja_apertura_id",
                table: "caja_cierres",
                column: "caja_apertura_id");

            migrationBuilder.CreateIndex(
                name: "IX_caja_aperturas_asignado_por",
                table: "caja_aperturas",
                column: "asignado_por");

            migrationBuilder.CreateIndex(
                name: "IX_caja_movimientos_caja_apertura_id",
                table: "caja_movimientos",
                column: "caja_apertura_id");

            migrationBuilder.CreateIndex(
                name: "IX_caja_movimientos_usuario_id",
                table: "caja_movimientos",
                column: "usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_caja_aperturas_usuarios_asignado_por",
                table: "caja_aperturas",
                column: "asignado_por",
                principalTable: "usuarios",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_caja_cierres_caja_aperturas_caja_apertura_id",
                table: "caja_cierres",
                column: "caja_apertura_id",
                principalTable: "caja_aperturas",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_caja_aperturas_caja_apertura_id",
                table: "ventas",
                column: "caja_apertura_id",
                principalTable: "caja_aperturas",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_caja_aperturas_usuarios_asignado_por",
                table: "caja_aperturas");

            migrationBuilder.DropForeignKey(
                name: "FK_caja_cierres_caja_aperturas_caja_apertura_id",
                table: "caja_cierres");

            migrationBuilder.DropForeignKey(
                name: "FK_ventas_caja_aperturas_caja_apertura_id",
                table: "ventas");

            migrationBuilder.DropTable(
                name: "caja_movimientos");

            migrationBuilder.DropIndex(
                name: "IX_ventas_caja_apertura_id",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_caja_cierres_caja_apertura_id",
                table: "caja_cierres");

            migrationBuilder.DropIndex(
                name: "IX_caja_aperturas_asignado_por",
                table: "caja_aperturas");

            migrationBuilder.DropColumn(
                name: "caja_apertura_id",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "billetes_q10",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "billetes_q100",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "billetes_q20",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "billetes_q200",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "billetes_q5",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "billetes_q50",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "caja_apertura_id",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "cantidad_ventas",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "devoluciones",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "diferencia",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "efectivo_contado",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "efectivo_esperado",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "entradas_efectivo",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "estado_corte",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "fondo_inicial",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "monedas",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "salidas_efectivo",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "tipo_corte",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "total_conteo",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "total_ventas",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "ventas_anuladas",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "ventas_cheque",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "ventas_credito",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "ventas_efectivo",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "ventas_mixto",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "ventas_tarjeta",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "ventas_transferencia",
                table: "caja_cierres");

            migrationBuilder.DropColumn(
                name: "asignado_por",
                table: "caja_aperturas");

            migrationBuilder.DropColumn(
                name: "observaciones",
                table: "caja_aperturas");

            migrationBuilder.AlterColumn<string>(
                name: "notas",
                table: "caja_cierres",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$IgFZw1ZgVMcj91qbGeaMRekUize2KfJg/Z5fFIukMqjeGklnroq4e");
        }
    }
}
