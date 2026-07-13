using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ApiAegis.Data;
using ApiAegis.Helpers;

namespace ApiAegis.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public ReportesController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        [TienePermiso("VerReportes")]
        public async Task<IActionResult> ObtenerIndicadoresDashboard()
        {
            var hoy = DateTime.UtcNow.Date;
            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // 1. Ventas del día
            var ventasDia = await _context.Ventas
                .Where(v => v.FechaVenta >= hoy && v.Estado == "PAGADA")
                .SumAsync(v => v.Total);

            // 2. Ventas del mes
            var ventasMes = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes && v.Estado == "PAGADA")
                .SumAsync(v => v.Total);

            // 3. Compras del mes
            var comprasMes = await _context.Compras
                .Where(c => c.FechaCompra >= inicioMes)
                .SumAsync(c => c.Total);

            // 4. Productos más vendidos
            var productosMasVendidos = await _context.VentasDetalle
                .Where(d => d.Venta!.Estado == "PAGADA")
                .GroupBy(d => new { d.ProductoId, d.Producto!.Nombre })
                .Select(g => new
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Nombre,
                    Cantidad = g.Sum(d => d.Cantidad),
                    TotalVendido = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToListAsync();

            // 5. Productos sin stock o con stock crítico (<= stock_minimo)
            var productosSinStock = await _context.Productos
                .Where(p => p.StockActual <= p.StockMinimo && p.Activo)
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Nombre,
                    p.StockActual,
                    p.StockMinimo
                })
                .ToListAsync();

            // 6. Ganancia diaria y mensual
            // Ganancia = Ventas total - costo total de los productos vendidos
            var detallesDia = await _context.VentasDetalle
                .Where(d => d.Venta!.FechaVenta >= hoy && d.Venta.Estado == "PAGADA")
                .Include(d => d.Producto)
                .ToListAsync();

            decimal gananciaDia = detallesDia.Sum(d => d.Subtotal - (d.Producto!.Costo * d.Cantidad));

            var detallesMes = await _context.VentasDetalle
                .Where(d => d.Venta!.FechaVenta >= inicioMes && d.Venta.Estado == "PAGADA")
                .Include(d => d.Producto)
                .ToListAsync();

            decimal gananciaMes = detallesMes.Sum(d => d.Subtotal - (d.Producto!.Costo * d.Cantidad));

            // 7. Clientes frecuentes
            var clientesFrecuentes = await _context.Ventas
                .Where(v => v.Estado == "PAGADA" && v.ClienteId != 1) // Omitir consumidor final
                .GroupBy(v => new { v.ClienteId, v.Cliente!.Nombre })
                .Select(g => new
                {
                    ClienteId = g.Key.ClienteId,
                    Nombre = g.Key.Nombre,
                    CantidadVentas = g.Count(),
                    TotalComprado = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.CantidadVentas)
                .Take(5)
                .ToListAsync();

            // 7.1 Ventas por Vendedor (del mes)
            var ventasPorVendedor = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes && v.Estado == "PAGADA")
                .GroupBy(v => new { v.UsuarioId, v.Usuario!.Nombre, v.Usuario.Apellido })
                .Select(g => new
                {
                    VendedorId = g.Key.UsuarioId,
                    Nombre = g.Key.Nombre + " " + g.Key.Apellido,
                    CantidadVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.TotalVendido)
                .ToListAsync();

            // 8. Aperturas y cierres recientes
            var aperturasRecientes = await _context.CajaAperturas
                .OrderByDescending(a => a.FechaApertura)
                .Take(5)
                .Select(a => new
                {
                    a.Id,
                    CajaNombre = a.Caja!.Nombre,
                    Usuario = $"{a.Usuario!.Nombre} {a.Usuario.Apellido}",
                    a.MontoInicial,
                    a.FechaApertura
                })
                .ToListAsync();

            var cierresRecientes = await _context.CajaCierres
                .OrderByDescending(c => c.FechaCierre)
                .Take(5)
                .Select(c => new
                {
                    c.Id,
                    CajaNombre = c.Caja!.Nombre,
                    Usuario = $"{c.Usuario!.Nombre} {c.Usuario.Apellido}",
                    c.MontoFinal,
                    c.FechaCierre
                })
                .ToListAsync();

            return Ok(new
            {
                ventasDia,
                ventasMes,
                gananciaDia,
                gananciaMes,
                comprasMes,
                productosMasVendidos,
                productosSinStock,
                clientesFrecuentes,
                ventasPorVendedor,
                aperturasRecientes,
                cierresRecientes
            });
        }
    }
}
