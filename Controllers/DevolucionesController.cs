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
    public class DevolucionesController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public DevolucionesController(AegisDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        // ============================================================
        //  CONSULTA
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DevolucionDto>>> GetDevoluciones(
            [FromQuery] int? ventaId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 50)
        {
            var query = _context.Devoluciones
                .AsNoTracking()
                .Include(d => d.Venta).ThenInclude(v => v!.Detalles)
                .Include(d => d.Usuario)
                .Include(d => d.Detalles).ThenInclude(dd => dd.Producto)
                .AsQueryable();

            if (ventaId.HasValue)
                query = query.Where(d => d.VentaId == ventaId.Value);

            var devoluciones = await query
                .OrderByDescending(d => d.Fecha)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return Ok(devoluciones.Select(Mapear).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DevolucionDto>> GetDevolucion(int id)
        {
            var devolucion = await _context.Devoluciones
                .AsNoTracking()
                .Include(d => d.Venta).ThenInclude(v => v!.Detalles)
                .Include(d => d.Usuario)
                .Include(d => d.Detalles).ThenInclude(dd => dd.Producto)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (devolucion == null)
                return NotFound(new { mensaje = "Devolución no encontrada" });

            return Ok(Mapear(devolucion));
        }

        [HttpGet("venta/{ventaId}")]
        public async Task<ActionResult<DevolucionVentaDto>> GetVentaParaDevolucion(int ventaId)
        {
            var venta = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Include(v => v.Cliente)
                .Include(v => v.MetodoPago)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
                return NotFound(new { mensaje = "Venta no encontrada" });

            if (venta.Estado == "ANULADA")
                return BadRequest(new { mensaje = "La venta ya se encuentra anulada, no admite devoluciones" });

            if (venta.Estado == "CREDITO")
                return BadRequest(new { mensaje = "La venta está a crédito con saldo pendiente. No admite devoluciones." });

            var devueltas = await _context.DevolucionesDetalle
                .AsNoTracking()
                .Where(dd => dd.Devolucion!.VentaId == ventaId)
                .GroupBy(dd => dd.ProductoId)
                .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToListAsync();
            var mapa = devueltas.ToDictionary(x => x.ProductoId, x => x.Cantidad);

            var dto = new DevolucionVentaDto
            {
                VentaId = venta.Id,
                NumeroDocumento = venta.NumeroDocumento,
                FechaVenta = venta.FechaVenta,
                ClienteNombre = !string.IsNullOrWhiteSpace(venta.NombreCliente)
                    ? venta.NombreCliente
                    : (venta.Cliente != null ? venta.Cliente.Nombre : "Consumidor final"),
                MetodoPago = venta.MetodoPago?.Nombre ?? "",
                Estado = venta.Estado,
                Total = venta.Total
            };

            foreach (var detalle in venta.Detalles)
            {
                var yaDevuelta = mapa.TryGetValue(detalle.ProductoId, out var c) ? c : 0;
                dto.Detalles.Add(new DevolucionVentaDetalleDto
                {
                    ProductoId = detalle.ProductoId,
                    ProductoNombre = detalle.Producto?.Nombre ?? "",
                    Codigo = detalle.Producto?.Codigo ?? "",
                    CantidadVendida = detalle.Cantidad,
                    CantidadDevuelta = yaDevuelta,
                    CantidadPendiente = Math.Max(0, detalle.Cantidad - yaDevuelta),
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal = detalle.Subtotal
                });
                dto.TotalDevuelto += yaDevuelta * PrecioUnitarioNeto(detalle);
            }

            dto.PendienteDevolver = Math.Max(0, dto.Total - dto.TotalDevuelto);
            return Ok(dto);
        }

        // ============================================================
        //  REGISTRO
        // ============================================================

        [HttpPost]
        [TienePermiso("GestionDevoluciones")]
        public async Task<IActionResult> CrearDevolucion([FromBody] CrearDevolucionDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Detalles == null || model.Detalles.Count == 0)
                return BadRequest(new { mensaje = "Debe especificar los productos a devolver" });

            var venta = await _context.Ventas
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == model.VentaId);

            if (venta == null)
                return NotFound(new { mensaje = "Venta no encontrada" });

            if (venta.Estado == "ANULADA")
                return BadRequest(new { mensaje = "La venta ya se encuentra anulada, no admite devoluciones" });

            if (venta.Estado == "CREDITO")
                return BadRequest(new { mensaje = "La venta está a crédito con saldo pendiente. No admite devoluciones; cobre o anule la venta primero." });

            var solicitadas = model.Detalles
                .Where(d => d.Cantidad > 0)
                .GroupBy(d => d.ProductoId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

            if (solicitadas.Count == 0)
                return BadRequest(new { mensaje = "Debe especificar cantidades mayores a 0" });

            var yaDevueltas = await _context.DevolucionesDetalle
                .AsNoTracking()
                .Where(dd => dd.Devolucion!.VentaId == venta.Id)
                .GroupBy(dd => dd.ProductoId)
                .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToListAsync();
            var mapaDevueltas = yaDevueltas.ToDictionary(x => x.ProductoId, x => x.Cantidad);

            decimal monto = 0;
            var errores = new List<string>();

            foreach (var kv in solicitadas)
            {
                var linea = venta.Detalles.FirstOrDefault(d => d.ProductoId == kv.Key);
                if (linea == null)
                {
                    errores.Add($"El producto {kv.Key} no pertenece a la venta {venta.NumeroDocumento}");
                    continue;
                }

                var ya = mapaDevueltas.TryGetValue(kv.Key, out var c) ? c : 0;
                var pendiente = linea.Cantidad - ya;
                if (kv.Value > pendiente)
                {
                    errores.Add($"'{linea.Producto?.Nombre}' solo tiene {pendiente} unidad(es) pendiente(s) por devolver");
                    continue;
                }

                monto += kv.Value * PrecioUnitarioNeto(linea);
            }

            if (errores.Count > 0)
                return BadRequest(new { mensaje = string.Join(" | ", errores) });

            monto = Math.Round(monto, 2);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var devolucion = new Devolucion
                {
                    VentaId = venta.Id,
                    UsuarioId = CurrentUserId,
                    Motivo = model.Motivo.Trim(),
                    MontoDevuelto = monto,
                    Fecha = DateTime.UtcNow
                };
                _context.Devoluciones.Add(devolucion);
                await _context.SaveChangesAsync();

                foreach (var kv in solicitadas)
                {
                    var linea = venta.Detalles.First(d => d.ProductoId == kv.Key);
                    var producto = linea.Producto;
                    if (producto == null)
                        continue;

                    int stockAnterior = producto.StockActual;
                    producto.StockActual += kv.Value;

                    _context.MovimientosInventario.Add(new MovimientoInventario
                    {
                        ProductoId = producto.Id,
                        TipoMovimiento = "DEVOLUCION",
                        Cantidad = kv.Value,
                        ExistenciaAnterior = stockAnterior,
                        ExistenciaNueva = producto.StockActual,
                        UsuarioId = CurrentUserId,
                        Observacion = $"Devolución venta {venta.NumeroDocumento} - {model.Motivo}",
                        FechaMovimiento = DateTime.UtcNow
                    });

                    devolucion.Detalles.Add(new DevolucionDetalle
                    {
                        ProductoId = producto.Id,
                        Cantidad = kv.Value
                    });
                }

                _context.Auditorias.Add(new Auditoria
                {
                    UsuarioId = CurrentUserId,
                    Accion = $"Registró devolución de la venta {venta.NumeroDocumento} por Q{monto:0.00} ({solicitadas.Count} producto(s)): {model.Motivo}",
                    TablaAfectada = "devoluciones",
                    RegistroId = devolucion.Id,
                    Fecha = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "Devolución registrada correctamente. Inventario actualizado.",
                    devolucionId = devolucion.Id,
                    montoDevuelto = devolucion.MontoDevuelto
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Ocurrió un error al registrar la devolución. Intente de nuevo." });
            }
        }

        // ============================================================
        //  HELPERS
        // ============================================================

        // Precio unitario neto de la línea de venta (descuento ya aplicado)
        private static decimal PrecioUnitarioNeto(VentaDetalle detalle) =>
            detalle.Cantidad > 0 ? Math.Round(detalle.Subtotal / detalle.Cantidad, 4) : detalle.PrecioUnitario;

        private static DevolucionDto Mapear(Devolucion d)
        {
            var lineas = d.Venta?.Detalles.ToDictionary(x => x.ProductoId) ?? new Dictionary<int, VentaDetalle>();

            return new DevolucionDto
            {
                Id = d.Id,
                VentaId = d.VentaId,
                VentaNumeroDocumento = d.Venta?.NumeroDocumento ?? "",
                UsuarioNombre = d.Usuario != null ? $"{d.Usuario.Nombre} {d.Usuario.Apellido}" : "",
                Motivo = d.Motivo,
                MontoDevuelto = d.MontoDevuelto,
                Fecha = d.Fecha,
                Detalles = d.Detalles.Select(dd =>
                {
                    var linea = lineas.TryGetValue(dd.ProductoId, out var v) ? v : null;
                    return new DevolucionDetalleDto
                    {
                        Id = dd.Id,
                        ProductoId = dd.ProductoId,
                        ProductoNombre = dd.Producto?.Nombre ?? "",
                        Cantidad = dd.Cantidad,
                        Subtotal = linea != null ? Math.Round(dd.Cantidad * PrecioUnitarioNeto(linea), 2) : 0m
                    };
                }).ToList()
            };
        }
    }
}
