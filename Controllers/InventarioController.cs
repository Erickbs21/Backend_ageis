using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ApiAegis.Data;
using ApiAegis.DTOs;
using ApiAegis.Models;
using ApiAegis.Helpers;

namespace ApiAegis.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public InventarioController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet("movimientos")]
        [TienePermiso("GestionInventario")]
        public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> GetMovimientos([FromQuery] string? tipo)
        {
            var query = _context.MovimientosInventario
                .Include(m => m.Producto)
                .Include(m => m.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(m => m.TipoMovimiento == tipo.ToUpperInvariant());

            var movimientos = await query
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => new MovimientoInventarioDto
                {
                    Id = m.Id,
                    ProductoId = m.ProductoId,
                    ProductoNombre = m.Producto!.Nombre,
                    TipoMovimiento = m.TipoMovimiento,
                    Cantidad = m.Cantidad,
                    ExistenciaAnterior = m.ExistenciaAnterior,
                    ExistenciaNueva = m.ExistenciaNueva,
                    UsuarioNombre = $"{m.Usuario!.Nombre} {m.Usuario.Apellido}",
                    Observacion = m.Observacion,
                    FechaMovimiento = m.FechaMovimiento
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        [HttpGet("movimientos/producto/{productoId}")]
        [TienePermiso("GestionInventario")]
        public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> GetMovimientosPorProducto(int productoId)
        {
            var movimientos = await _context.MovimientosInventario
                .Where(m => m.ProductoId == productoId)
                .Include(m => m.Producto)
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => new MovimientoInventarioDto
                {
                    Id = m.Id,
                    ProductoId = m.ProductoId,
                    ProductoNombre = m.Producto!.Nombre,
                    TipoMovimiento = m.TipoMovimiento,
                    Cantidad = m.Cantidad,
                    ExistenciaAnterior = m.ExistenciaAnterior,
                    ExistenciaNueva = m.ExistenciaNueva,
                    UsuarioNombre = $"{m.Usuario!.Nombre} {m.Usuario.Apellido}",
                    Observacion = m.Observacion,
                    FechaMovimiento = m.FechaMovimiento
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        [HttpGet("stock-bajo")]
        [TienePermiso("GestionInventario")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetStockBajo()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.StockActual <= p.StockMinimo && p.Activo)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    CodigoBarras = p.CodigoBarras,
                    Nombre = p.Nombre,
                    CategoriaNombre = p.Categoria!.Nombre,
                    StockMinimo = p.StockMinimo,
                    StockActual = p.StockActual
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpPost("ajuste")]
        [TienePermiso("GestionInventario")]
        public async Task<IActionResult> AjustarStock([FromBody] RegistrarMovimientoManualDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tipo = model.TipoMovimiento.ToUpper();
            if (tipo != "ENTRADA" && tipo != "SALIDA" && tipo != "AJUSTE")
            {
                return BadRequest(new { mensaje = "Tipo de movimiento inválido. Debe ser ENTRADA, SALIDA o AJUSTE" });
            }

            var producto = await _context.Productos.FindAsync(model.ProductoId);
            if (producto == null || !producto.Activo)
                return NotFound(new { mensaje = "Producto no encontrado o inactivo" });

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            int stockAnterior = producto.StockActual;
            int stockNuevo = stockAnterior;

            if (tipo == "ENTRADA")
            {
                stockNuevo += model.Cantidad;
            }
            else if (tipo == "SALIDA")
            {
                if (stockAnterior < model.Cantidad)
                {
                    return BadRequest(new { mensaje = "No hay suficiente existencia para realizar la salida" });
                }
                stockNuevo -= model.Cantidad;
            }
            else // AJUSTE
            {
                // Para el ajuste manual el model.Cantidad será el valor real absoluto deseado en stock
                stockNuevo = model.Cantidad;
            }

            producto.StockActual = stockNuevo;

            var movimiento = new MovimientoInventario
            {
                ProductoId = producto.Id,
                TipoMovimiento = tipo,
                Cantidad = Math.Abs(stockNuevo - stockAnterior),
                ExistenciaAnterior = stockAnterior,
                ExistenciaNueva = stockNuevo,
                UsuarioId = currentUserId,
                Observacion = model.Observacion ?? "Ajuste manual de inventario",
                FechaMovimiento = DateTime.UtcNow
            };

            _context.MovimientosInventario.Add(movimiento);

            // Auditoría
            var audit = new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Realizó ajuste de inventario ({tipo}) para producto {producto.Nombre}. Anterior: {stockAnterior}, Nuevo: {stockNuevo}",
                TablaAfectada = "productos",
                RegistroId = producto.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ajuste de inventario realizado correctamente", stockActual = stockNuevo });
        }

        // ============================================================
        //  INVENTARIO FISICO (conteo vs sistema)
        // ============================================================

        [HttpPost("fisico")]
        [TienePermiso("GestionInventario")]
        public async Task<ActionResult<InventarioFisicoResultadoDto>> RegistrarInventarioFisico([FromBody] InventarioFisicoDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Detalles == null || model.Detalles.Count == 0)
                return BadRequest(new { mensaje = "Debe indicar los productos contados" });

            var ids = model.Detalles.Select(d => d.ProductoId).Distinct().ToList();
            if (ids.Count != model.Detalles.Count)
                return BadRequest(new { mensaje = "Hay productos repetidos en el conteo" });

            // El proveedor MySQL no traduce colecciones primitivas: se filtra en memoria
            var productos = (await _context.Productos.ToListAsync())
                .Where(p => ids.Contains(p.Id))
                .ToList();

            var faltantes = ids.Where(id => !productos.Any(p => p.Id == id)).ToList();
            if (faltantes.Count > 0)
                return BadRequest(new { mensaje = $"Producto(s) no encontrado(s): {string.Join(", ", faltantes)}" });

            var motivo = model.Motivo.Trim();
            var conteo = model.Detalles.ToDictionary(d => d.ProductoId, d => d.CantidadContada);
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var resultado = new InventarioFisicoResultadoDto();

            foreach (var producto in productos)
            {
                var contada = conteo[producto.Id];
                int anterior = producto.StockActual;
                int diferencia = contada - anterior;

                resultado.Revisados++;

                if (diferencia == 0)
                {
                    resultado.SinDiferencia++;
                    continue;
                }

                producto.StockActual = contada;

                _context.MovimientosInventario.Add(new MovimientoInventario
                {
                    ProductoId = producto.Id,
                    TipoMovimiento = "AJUSTE",
                    Cantidad = Math.Abs(diferencia),
                    ExistenciaAnterior = anterior,
                    ExistenciaNueva = contada,
                    UsuarioId = currentUserId,
                    Observacion = $"Inventario físico: contado {contada} (diferencia {(diferencia > 0 ? "+" : "")}{diferencia}) - {motivo}",
                    FechaMovimiento = DateTime.UtcNow
                });

                resultado.Ajustados++;
                resultado.Ajustes.Add(new InventarioFisicoAjusteDto
                {
                    ProductoId = producto.Id,
                    ProductoNombre = producto.Nombre,
                    Codigo = producto.Codigo,
                    StockAnterior = anterior,
                    StockContado = contada,
                    Diferencia = diferencia
                });
            }

            if (resultado.Ajustados > 0)
            {
                var resumen = string.Join(", ",
                    resultado.Ajustes.Take(10).Select(a =>
                        $"{a.ProductoNombre}: {a.StockAnterior}->{a.StockContado} ({a.Diferencia})"));

                _context.Auditorias.Add(new Auditoria
                {
                    UsuarioId = currentUserId,
                    Accion = $"Registró inventario físico con {resultado.Ajustados} ajuste(s). {resumen} | Motivo: {motivo}",
                    TablaAfectada = "productos",
                    RegistroId = resultado.Ajustes[0].ProductoId,
                    Fecha = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(resultado);
        }
    }
}
