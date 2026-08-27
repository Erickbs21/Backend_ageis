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
    public class VentasController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public VentasController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentas([FromQuery] int pagina = 1, [FromQuery] int tamano = 50)
        {
            var ventas = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Pagos)
                    .ThenInclude(p => p.MetodoPago)
                .OrderByDescending(v => v.FechaVenta)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(v => new VentaDto
                {
                    Id = v.Id,
                    NumeroDocumento = v.NumeroDocumento,
                    ClienteId = v.ClienteId,
                    ClienteNombre = v.Cliente!.Nombre,
                    NombreCliente = v.NombreCliente,
                    Nit = v.Nit,
                    TipoDocumento = v.TipoDocumento,
                    UsuarioId = v.UsuarioId,
                    UsuarioNombre = $"{v.Usuario!.Nombre} {v.Usuario.Apellido}",
                    Subtotal = v.Subtotal,
                    Descuento = v.Descuento,
                    Impuestos = v.Impuestos,
                    Total = v.Total,
                    Vuelto = v.Vuelto,
                    Estado = v.Estado,
                    FechaVenta = v.FechaVenta,
                    Pagos = v.Pagos.Select(p => new VentaPagoDto
                    {
                        Id = p.Id,
                        MetodoPagoId = p.MetodoPagoId,
                        MetodoPagoNombre = p.MetodoPago!.Nombre,
                        Monto = p.Monto,
                        FechaPago = p.FechaPago
                    }).ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDto>> GetVenta(int id)
        {
            var v = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Pagos)
                    .ThenInclude(p => p.MetodoPago)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (v == null)
                return NotFound(new { mensaje = "Venta no encontrada" });

            var dto = new VentaDto
            {
                Id = v.Id,
                NumeroDocumento = v.NumeroDocumento,
                ClienteId = v.ClienteId,
                ClienteNombre = v.Cliente!.Nombre,
                NombreCliente = v.NombreCliente,
                Nit = v.Nit,
                TipoDocumento = v.TipoDocumento,
                UsuarioId = v.UsuarioId,
                UsuarioNombre = $"{v.Usuario!.Nombre} {v.Usuario.Apellido}",
                Subtotal = v.Subtotal,
                Descuento = v.Descuento,
                Impuestos = v.Impuestos,
                Total = v.Total,
                Vuelto = v.Vuelto,
                Estado = v.Estado,
                FechaVenta = v.FechaVenta,
                Pagos = v.Pagos.Select(p => new VentaPagoDto
                {
                    Id = p.Id,
                    MetodoPagoId = p.MetodoPagoId,
                    MetodoPagoNombre = p.MetodoPago!.Nombre,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago
                }).ToList(),
                Detalles = v.Detalles.Select(d => new VentaDetalleDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto!.Nombre,
                    ProductoCodigo = d.Producto.Codigo,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Descuento = d.Descuento,
                    Subtotal = d.Subtotal
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        [TienePermiso("CrearVentas")]
        public async Task<ActionResult<VentaDto>> CrearVenta([FromBody] CrearVentaDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Verificar si la caja está abierta
            var cajaAbierta = await _context.Cajas.AnyAsync(c => c.Estado == "Abierta");
            if (!cajaAbierta)
                return BadRequest(new { mensaje = "Debe realizar la apertura de caja antes de registrar ventas" });

            // 2. Verificar Cliente
            var cliente = await _context.Clientes.FindAsync(model.ClienteId);
            if (cliente == null || !cliente.Activo)
                return BadRequest(new { mensaje = "El cliente especificado no existe o está inactivo" });

            // NIT de la venta
            string? nitFinal = string.IsNullOrWhiteSpace(model.Nit) ? cliente.Nit : model.Nit!.Trim();

            // Si es Consumidor Final (1) pero el NIT coincide con un cliente registrado,
            // vincular la venta a ese cliente para mostrar sus datos en el historial.
            int clienteIdFinal = model.ClienteId;
            if (clienteIdFinal == 1 && !string.IsNullOrWhiteSpace(nitFinal))
            {
                var clientePorNit = await _context.Clientes.FirstOrDefaultAsync(c => c.Nit == nitFinal && c.Activo);
                if (clientePorNit != null)
                {
                    clienteIdFinal = clientePorNit.Id;
                    cliente = clientePorNit;
                    nitFinal = clientePorNit.Nit;
                }
            }

            // 3. Verificar Método de Pago (si no usa array Pagos)
            int metodoPagoPrincipal = 1; // Default a efectivo
            if (model.Pagos.Any())
            {
                metodoPagoPrincipal = model.Pagos.First().MetodoPagoId;
            }

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            // Iniciar transacción de base de datos
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = new Venta
                {
                    NumeroDocumento = $"VNT-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(100, 999)}",
                    ClienteId = clienteIdFinal,
                    NombreCliente = model.NombreCliente,
                    Nit = nitFinal,
                    TipoDocumento = model.TipoDocumento,
                    UsuarioId = currentUserId,
                    MetodoPagoId = metodoPagoPrincipal,
                    Descuento = model.Descuento,
                    Estado = model.TipoDocumento == "VENTA" ? "PAGADA" : "PENDIENTE",
                    FechaVenta = DateTime.UtcNow
                };

                decimal subtotalAcumulado = 0.00m;

                foreach (var item in model.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null || !producto.Activo)
                    {
                        return BadRequest(new { mensaje = $"El producto con ID {item.ProductoId} no existe o está inactivo" });
                    }

                    // Verificar stock
                    if (producto.StockActual < item.Cantidad)
                    {
                        return BadRequest(new { mensaje = $"Stock insuficiente para el producto: {producto.Nombre}. Stock actual: {producto.StockActual}, solicitado: {item.Cantidad}" });
                    }

                    decimal detalleSubtotal = (item.PrecioUnitario * item.Cantidad) - item.Descuento;
                    subtotalAcumulado += (item.PrecioUnitario * item.Cantidad);

                    var detalle = new VentaDetalle
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Descuento = item.Descuento,
                        Subtotal = detalleSubtotal
                    };

                    venta.Detalles.Add(detalle);

                    // Descontar inventario y registrar movimiento
                    int stockAnterior = producto.StockActual;
                    producto.StockActual -= item.Cantidad;

                    var movimiento = new MovimientoInventario
                    {
                        ProductoId = producto.Id,
                        TipoMovimiento = "SALIDA",
                        Cantidad = item.Cantidad,
                        ExistenciaAnterior = stockAnterior,
                        ExistenciaNueva = producto.StockActual,
                        UsuarioId = currentUserId,
                        Observacion = $"Venta registrada - Doc: {venta.NumeroDocumento}",
                        FechaMovimiento = DateTime.UtcNow
                    };
                    _context.MovimientosInventario.Add(movimiento);
                }

                venta.Subtotal = subtotalAcumulado;
                // Impuestos (ej. IVA 12% incluido)
                venta.Impuestos = (subtotalAcumulado - model.Descuento) * 0.12m;
                venta.Total = subtotalAcumulado - model.Descuento;

                // Calcular Vuelto y Pagos
                decimal totalPagado = 0;
                foreach(var pagoModel in model.Pagos)
                {
                    totalPagado += pagoModel.Monto;
                    venta.Pagos.Add(new VentaPago
                    {
                        MetodoPagoId = pagoModel.MetodoPagoId,
                        Monto = pagoModel.Monto
                    });
                }

                if(totalPagado > venta.Total && model.TipoDocumento == "VENTA")
                {
                    venta.Vuelto = totalPagado - venta.Total;
                }

                _context.Ventas.Add(venta);

                // Auditoría
                var audit = new Auditoria
                {
                    UsuarioId = currentUserId,
                    Accion = $"Registró {venta.TipoDocumento.ToLower()} {venta.NumeroDocumento} por total de {venta.Total}",
                    TablaAfectada = "ventas",
                    Fecha = DateTime.UtcNow
                };
                _context.Auditorias.Add(audit);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Calcular Faltante para el DTO
                decimal faltante = 0;
                if(venta.TipoDocumento != "COTIZACION" && totalPagado < venta.Total)
                {
                    faltante = venta.Total - totalPagado;
                }

                // Retornar DTO
                var resultadoDto = new VentaDto
                {
                    Id = venta.Id,
                    NumeroDocumento = venta.NumeroDocumento,
                    ClienteId = venta.ClienteId,
                    ClienteNombre = cliente.Nombre,
                    NombreCliente = venta.NombreCliente,
                    Nit = venta.Nit,
                    TipoDocumento = venta.TipoDocumento,
                    UsuarioId = venta.UsuarioId,
                    UsuarioNombre = User.Identity?.Name ?? "",
                    Subtotal = venta.Subtotal,
                    Descuento = venta.Descuento,
                    Impuestos = venta.Impuestos,
                    Total = venta.Total,
                    Vuelto = venta.Vuelto,
                    Faltante = faltante,
                    Estado = venta.Estado,
                    FechaVenta = venta.FechaVenta
                };

                return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, resultadoDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Ocurrió un error al registrar la venta", error = ex.Message });
            }
        }

        [HttpPost("{id}/anular")]
        [TienePermiso("AnularVentas")]
        public async Task<IActionResult> AnularVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound(new { mensaje = "Venta no encontrada" });

            if (venta.Estado == "ANULADA")
                return BadRequest(new { mensaje = "La venta ya se encuentra anulada" });

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                venta.Estado = "ANULADA";

                // Devolver stock
                foreach (var detalle in venta.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        int stockAnterior = producto.StockActual;
                        producto.StockActual += detalle.Cantidad;

                        var movimiento = new MovimientoInventario
                        {
                            ProductoId = producto.Id,
                            TipoMovimiento = "DEVOLUCION",
                            Cantidad = detalle.Cantidad,
                            ExistenciaAnterior = stockAnterior,
                            ExistenciaNueva = producto.StockActual,
                            UsuarioId = currentUserId,
                            Observacion = $"Anulación de venta - Doc: {venta.NumeroDocumento}",
                            FechaMovimiento = DateTime.UtcNow
                        };
                        _context.MovimientosInventario.Add(movimiento);
                    }
                }

                // Auditoría
                var audit = new Auditoria
                {
                    UsuarioId = currentUserId,
                    Accion = $"Anuló la venta {venta.NumeroDocumento}",
                    TablaAfectada = "ventas",
                    RegistroId = venta.Id,
                    Fecha = DateTime.UtcNow
                };
                _context.Auditorias.Add(audit);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Venta anulada y existencias restauradas correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Ocurrió un error al anular la venta", error = ex.Message });
            }
        }
    }
}
