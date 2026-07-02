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
        public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> GetMovimientos()
        {
            var movimientos = await _context.MovimientosInventario
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
    }
}
