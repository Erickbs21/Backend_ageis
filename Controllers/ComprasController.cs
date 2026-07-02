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
    public class ComprasController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public ComprasController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompraDto>>> GetCompras()
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaCompra)
                .Select(c => new CompraDto
                {
                    Id = c.Id,
                    ProveedorId = c.ProveedorId,
                    ProveedorNombre = c.Proveedor!.Nombre,
                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = $"{c.Usuario!.Nombre} {c.Usuario.Apellido}",
                    Subtotal = c.Subtotal,
                    Impuestos = c.Impuestos,
                    Total = c.Total,
                    FechaCompra = c.FechaCompra
                })
                .ToListAsync();

            return Ok(compras);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompraDto>> GetCompra(int id)
        {
            var c = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null)
                return NotFound(new { mensaje = "Compra no encontrada" });

            var dto = new CompraDto
            {
                Id = c.Id,
                ProveedorId = c.ProveedorId,
                ProveedorNombre = c.Proveedor!.Nombre,
                UsuarioId = c.UsuarioId,
                UsuarioNombre = $"{c.Usuario!.Nombre} {c.Usuario.Apellido}",
                Subtotal = c.Subtotal,
                Impuestos = c.Impuestos,
                Total = c.Total,
                FechaCompra = c.FechaCompra,
                Detalles = c.Detalles.Select(d => new CompraDetalleDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto!.Nombre,
                    Cantidad = d.Cantidad,
                    CostoUnitario = d.CostoUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        [TienePermiso("GestionInventario")]
        public async Task<ActionResult<CompraDto>> CrearCompra([FromBody] CrearCompraDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var proveedor = await _context.Proveedores.FindAsync(model.ProveedorId);
            if (proveedor == null || !proveedor.Activo)
                return BadRequest(new { mensaje = "El proveedor especificado no existe o está inactivo" });

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var compra = new Compra
                {
                    ProveedorId = model.ProveedorId,
                    UsuarioId = currentUserId,
                    FechaCompra = DateTime.UtcNow
                };

                decimal subtotalAcumulado = 0.00m;

                foreach (var item in model.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null || !producto.Activo)
                        return BadRequest(new { mensaje = $"El producto con ID {item.ProductoId} no existe o está inactivo" });

                    decimal detalleSubtotal = item.CostoUnitario * item.Cantidad;
                    subtotalAcumulado += detalleSubtotal;

                    var detalle = new CompraDetalle
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        CostoUnitario = item.CostoUnitario,
                        Subtotal = detalleSubtotal
                    };

                    compra.Detalles.Add(detalle);

                    // Aumentar stock y actualizar el costo del producto en catálogo
                    int stockAnterior = producto.StockActual;
                    producto.StockActual += item.Cantidad;
                    producto.Costo = item.CostoUnitario; // Actualizar costo de adquisición al último costo de compra

                    var movimiento = new MovimientoInventario
                    {
                        ProductoId = producto.Id,
                        TipoMovimiento = "ENTRADA",
                        Cantidad = item.Cantidad,
                        ExistenciaAnterior = stockAnterior,
                        ExistenciaNueva = producto.StockActual,
                        UsuarioId = currentUserId,
                        Observacion = $"Ingreso por Compra de Mercadería",
                        FechaMovimiento = DateTime.UtcNow
                    };
                    _context.MovimientosInventario.Add(movimiento);
                }

                compra.Subtotal = subtotalAcumulado;
                compra.Impuestos = subtotalAcumulado * 0.12m; // IVA del 12%
                compra.Total = subtotalAcumulado + compra.Impuestos;

                _context.Compras.Add(compra);

                // Auditoría
                var audit = new Auditoria
                {
                    UsuarioId = currentUserId,
                    Accion = $"Registró compra por total de {compra.Total}",
                    TablaAfectada = "compras",
                    Fecha = DateTime.UtcNow
                };
                _context.Auditorias.Add(audit);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var resultadoDto = new CompraDto
                {
                    Id = compra.Id,
                    ProveedorId = compra.ProveedorId,
                    ProveedorNombre = proveedor.Nombre,
                    UsuarioId = compra.UsuarioId,
                    UsuarioNombre = User.Identity?.Name ?? "",
                    Subtotal = compra.Subtotal,
                    Impuestos = compra.Impuestos,
                    Total = compra.Total,
                    FechaCompra = compra.FechaCompra
                };

                return CreatedAtAction(nameof(GetCompra), new { id = compra.Id }, resultadoDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Ocurrió un error al registrar la compra", error = ex.Message });
            }
        }
    }
}
