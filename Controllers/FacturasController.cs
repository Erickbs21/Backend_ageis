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
    public class FacturasController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public FacturasController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacturaDto>>> GetFacturas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.FechaVenta)
                .Select(v => new FacturaDto
                {
                    Id = v.Facturas.OrderByDescending(f => f.Id).FirstOrDefault() != null
                        ? v.Facturas.OrderByDescending(f => f.Id).First().Id : 0,
                    VentaId = v.Id,
                    NumeroDocumento = v.NumeroDocumento,
                    ClienteNombre = v.Cliente != null ? v.Cliente.Nombre : (v.NombreCliente ?? "CF"),
                    Total = v.Total,
                    Serie = v.Facturas.OrderByDescending(f => f.Id).FirstOrDefault() != null
                        ? v.Facturas.OrderByDescending(f => f.Id).First().Serie : "",
                    Numero = v.Facturas.OrderByDescending(f => f.Id).FirstOrDefault() != null
                        ? v.Facturas.OrderByDescending(f => f.Id).First().Numero : "",
                    Uuid = v.Facturas.OrderByDescending(f => f.Id).FirstOrDefault() != null
                        ? v.Facturas.OrderByDescending(f => f.Id).First().Uuid : "",
                    Estado = v.Facturas.OrderByDescending(f => f.Id).FirstOrDefault() != null
                        ? v.Facturas.OrderByDescending(f => f.Id).First().Estado : "SIN FACTURA",
                    FechaEmision = v.FechaVenta
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<FacturaDto>>> BuscarFacturas([FromQuery] string? serie, [FromQuery] string? numero, [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] string? clienteQuery)
        {
            var query = _context.Facturas.Include(f => f.Venta).ThenInclude(v => v!.Cliente).AsQueryable();

            if (!string.IsNullOrEmpty(serie))
                query = query.Where(f => f.Serie.ToLower() == serie.ToLower());

            if (!string.IsNullOrEmpty(numero))
                query = query.Where(f => f.Numero.Contains(numero));

            if (fechaInicio.HasValue)
                query = query.Where(f => f.FechaEmision >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(f => f.FechaEmision <= fechaFin.Value.AddDays(1));

            if (!string.IsNullOrEmpty(clienteQuery))
            {
                clienteQuery = clienteQuery.ToLower();
                query = query.Where(f => f.Venta!.Cliente!.Nombre.ToLower().Contains(clienteQuery) || f.Venta!.Cliente!.Nit.ToLower().Contains(clienteQuery) || (f.Venta!.Cliente!.Dpi != null && f.Venta.Cliente.Dpi.ToLower().Contains(clienteQuery)));
            }

            var facturas = await query
                .OrderByDescending(f => f.FechaEmision)
                .Select(f => new FacturaDto
                {
                    Id = f.Id,
                    VentaId = f.VentaId,
                    NumeroDocumento = f.Venta!.NumeroDocumento,
                    ClienteNombre = f.Venta.Cliente != null ? f.Venta.Cliente.Nombre : (f.Venta.NombreCliente ?? "CF"),
                    Total = f.Venta.Total,
                    Serie = f.Serie,
                    Numero = f.Numero,
                    Uuid = f.Uuid,
                    Estado = f.Estado,
                    FechaEmision = f.FechaEmision
                })
                .ToListAsync();

            return Ok(facturas);
        }

        [HttpGet("ultima-secuencia")]
        public async Task<ActionResult> GetUltimaSecuencia([FromQuery] string serie = "A")
        {
            var ultima = await _context.Facturas
                .Where(f => f.Serie == serie)
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync();

            if (ultima == null)
                return Ok(new { serie, ultimoNumero = "No hay facturas emitidas en esta serie" });

            return Ok(new { serie, ultimoNumero = ultima.Numero });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacturaDto>> GetFactura(int id)
        {
            var f = await _context.Facturas
                .Include(f => f.Venta).ThenInclude(v => v!.Cliente)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (f == null)
                return NotFound(new { mensaje = "Factura no encontrada" });

            return Ok(new FacturaDto
            {
                Id = f.Id,
                VentaId = f.VentaId,
                NumeroDocumento = f.Venta!.NumeroDocumento,
                ClienteNombre = f.Venta.Cliente != null ? f.Venta.Cliente.Nombre : (f.Venta.NombreCliente ?? "CF"),
                Total = f.Venta.Total,
                Serie = f.Serie,
                Numero = f.Numero,
                Uuid = f.Uuid,
                Estado = f.Estado,
                FechaEmision = f.FechaEmision
            });
        }

        [HttpPost("generar/{ventaId}")]
        [TienePermiso("CrearVentas")]
        public async Task<ActionResult<FacturaDto>> GenerarFactura(int ventaId)
        {
            var venta = await _context.Ventas.FindAsync(ventaId);
            if (venta == null)
                return NotFound(new { mensaje = "La venta no existe" });

            var existeFactura = await _context.Facturas.AnyAsync(f => f.VentaId == ventaId && f.Estado == "EMITIDA");
            if (existeFactura)
                return BadRequest(new { mensaje = "Ya existe una factura emitida para esta venta" });

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var factura = new Factura
            {
                VentaId = ventaId,
                Serie = "A",
                Numero = new Random().Next(100000, 999999).ToString(),
                Uuid = Guid.NewGuid().ToString().ToUpper(),
                Estado = "EMITIDA",
                FechaEmision = DateTime.UtcNow
            };

            _context.Facturas.Add(factura);

            var audit = new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Generó factura Serie: {factura.Serie} Número: {factura.Numero} para venta ID {ventaId}",
                TablaAfectada = "facturas",
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFactura), new { id = factura.Id }, new FacturaDto
            {
                Id = factura.Id,
                VentaId = factura.VentaId,
                NumeroDocumento = venta.NumeroDocumento,
                ClienteNombre = venta.NombreCliente ?? "CF",
                Total = venta.Total,
                Serie = factura.Serie,
                Numero = factura.Numero,
                Uuid = factura.Uuid,
                Estado = factura.Estado,
                FechaEmision = factura.FechaEmision
            });
        }
    }
}
