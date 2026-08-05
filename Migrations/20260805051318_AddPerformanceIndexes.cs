using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiAegis.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ventas_cliente_id",
                table: "ventas",
                newName: "IX_Ventas_ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_movimientos_inventario_producto_id",
                table: "movimientos_inventario",
                newName: "IX_MovimientosInventario_ProductoId");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$pqKQZKvxHp5h6frZ08OAfOvNpYM43K/G46zctwf9QWzp6kLeZMnQ2");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Estado",
                table: "ventas",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_FechaVenta",
                table: "ventas",
                column: "fecha_venta");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Nit",
                table: "proveedores",
                column: "nit");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "productos",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoBarras",
                table: "productos",
                column: "codigo_barras");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre",
                table: "productos",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_StockActual",
                table: "productos",
                column: "stock_actual");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FechaMovimiento",
                table: "movimientos_inventario",
                column: "fecha_movimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Nit",
                table: "clientes",
                column: "nit");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Nombre",
                table: "clientes",
                column: "nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ventas_Estado",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_FechaVenta",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_Nit",
                table: "proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_CodigoBarras",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Nombre",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_StockActual",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_FechaMovimiento",
                table: "movimientos_inventario");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Nit",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Nombre",
                table: "clientes");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_ClienteId",
                table: "ventas",
                newName: "IX_ventas_cliente_id");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosInventario_ProductoId",
                table: "movimientos_inventario",
                newName: "IX_movimientos_inventario_producto_id");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$VKY9V3mrtKvxyXxG8aeSVei0joFJMyE3RlirgSuC1PaV6DoujY37O");
        }
    }
}
