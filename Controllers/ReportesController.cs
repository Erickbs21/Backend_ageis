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

            // 2.1 Descuentos aplicados (día y mes)
            var descuentosDia = await _context.Ventas
                .Where(v => v.FechaVenta >= hoy && v.Estado == "PAGADA")
                .SumAsync(v => v.Descuento);

            var descuentosMes = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes && v.Estado == "PAGADA")
                .SumAsync(v => v.Descuento);

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
                descuentosDia,
                descuentosMes,
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

        // ============================================================
        //  REPORTE DE VENTAS POR PERIODO
        // ============================================================

        [HttpGet("ventas")]
        [TienePermiso("VerReportes")]
        public async Task<IActionResult> ReporteVentas(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int? usuarioId,
            [FromQuery] string? estado)
        {
            var fechaDesde = DateTime.SpecifyKind((desde ?? DateTime.UtcNow.Date.AddDays(-29)).Date, DateTimeKind.Utc);
            var fechaHasta = DateTime.SpecifyKind((hasta ?? DateTime.UtcNow.Date).Date.AddDays(1), DateTimeKind.Utc);

            if (fechaHasta <= fechaDesde)
                return BadRequest(new { mensaje = "El rango de fechas es inválido (hasta debe ser mayor o igual a desde)" });

            var query = _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Pagos)
                    .ThenInclude(p => p.MetodoPago)
                .Where(v => v.FechaVenta >= fechaDesde && v.FechaVenta < fechaHasta);

            if (usuarioId.HasValue)
                query = query.Where(v => v.UsuarioId == usuarioId.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(v => v.Estado == estado.ToUpperInvariant());

            var ventas = await query.OrderByDescending(v => v.FechaVenta).ToListAsync();

            var devolucionesPeriodo = await _context.Devoluciones
                .AsNoTracking()
                .Where(d => d.Fecha >= fechaDesde && d.Fecha < fechaHasta)
                .SumAsync(d => d.MontoDevuelto);

            var filas = ventas.Select(v =>
            {
                decimal costo = v.Detalles.Sum(d => (d.Producto?.Costo ?? 0) * d.Cantidad);
                decimal ingreso = v.Estado == "PAGADA" ? v.Total : 0;
                return new
                {
                    Id = v.Id,
                    NumeroDocumento = v.NumeroDocumento,
                    FechaVenta = v.FechaVenta,
                    ClienteNombre = v.Cliente?.Nombre ?? v.NombreCliente ?? "CF",
                    UsuarioNombre = $"{v.Usuario?.Nombre} {v.Usuario?.Apellido}".Trim(),
                    Estado = v.Estado,
                    Subtotal = v.Subtotal,
                    Descuento = v.Descuento,
                    Impuestos = v.Impuestos,
                    Total = v.Total,
                    Costo = costo,
                    Ganancia = ingreso - costo
                };
            }).ToList();

            var pagadas = ventas.Where(v => v.Estado == "PAGADA").ToList();

            var porMetodoPago = pagadas
                .SelectMany(v => v.Pagos.Select(p => new { Metodo = p.MetodoPago?.Nombre ?? "Sin método", p.Monto }))
                .GroupBy(x => x.Metodo)
                .Select(g => new { Metodo = g.Key, Monto = g.Sum(x => x.Monto) })
                .OrderByDescending(x => x.Monto)
                .ToList();

            var porVendedor = pagadas
                .GroupBy(v => new { v.UsuarioId, Nombre = $"{v.Usuario?.Nombre} {v.Usuario?.Apellido}".Trim() })
                .Select(g => new
                {
                    VendedorId = g.Key.UsuarioId,
                    g.Key.Nombre,
                    Cantidad = g.Count(),
                    Monto = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.Monto)
                .ToList();

            return Ok(new
            {
                desde = fechaDesde,
                hasta = fechaHasta.AddDays(-1),
                resumen = new
                {
                    CantidadVentas = ventas.Count,
                    MontoVentas = pagadas.Sum(v => v.Total),
                    MontoDescuentos = pagadas.Sum(v => v.Descuento),
                    MontoDevoluciones = devolucionesPeriodo,
                    MontoImpuestos = pagadas.Sum(v => v.Impuestos),
                    MontoGanancia = pagadas.Sum(v => v.Total) - pagadas.Sum(v => v.Detalles.Sum(d => (d.Producto?.Costo ?? 0) * d.Cantidad)),
                    PorMetodoPago = porMetodoPago,
                    PorVendedor = porVendedor
                },
                ventas = filas
            });
        }

        // ============================================================
        //  REPORTE DE PRODUCTOS VENDIDOS POR PERIODO
        // ============================================================

        [HttpGet("productos")]
        [TienePermiso("VerReportes")]
        public async Task<IActionResult> ReporteProductos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var fechaDesde = DateTime.SpecifyKind((desde ?? DateTime.UtcNow.Date.AddDays(-29)).Date, DateTimeKind.Utc);
            var fechaHasta = DateTime.SpecifyKind((hasta ?? DateTime.UtcNow.Date).Date.AddDays(1), DateTimeKind.Utc);

            if (fechaHasta <= fechaDesde)
                return BadRequest(new { mensaje = "El rango de fechas es inválido (hasta debe ser mayor o igual a desde)" });

            var detalles = await _context.VentasDetalle
                .AsNoTracking()
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.FechaVenta >= fechaDesde
                    && d.Venta.FechaVenta < fechaHasta
                    && d.Venta.Estado == "PAGADA")
                .ToListAsync();

            var productos = detalles
                .GroupBy(d => new { d.ProductoId, d.Producto!.Codigo, d.Producto!.Nombre })
                .Select(g => new
                {
                    ProductoId = g.Key.ProductoId,
                    g.Key.Codigo,
                    g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    Ingreso = g.Sum(d => d.Subtotal),
                    Costo = g.Sum(d => (d.Producto?.Costo ?? 0) * d.Cantidad),
                    Ganancia = g.Sum(d => d.Subtotal) - g.Sum(d => (d.Producto?.Costo ?? 0) * d.Cantidad)
                })
                .OrderByDescending(x => x.Ingreso)
                .ToList();

            return Ok(new
            {
                desde = fechaDesde,
                hasta = fechaHasta.AddDays(-1),
                productos
            });
        }
    }
}
