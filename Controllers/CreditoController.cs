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
    public class CreditoController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public CreditoController(AegisDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        // ============================================================
        //  CARTERA DE CRÉDITO (clientes con saldo pendiente)
        // ============================================================

        [HttpGet("cartera")]
        public async Task<ActionResult<IEnumerable<CarteraCreditoDto>>> GetCartera()
        {
            var ventasCredito = await _context.Ventas.AsNoTracking()
                .Where(v => v.Estado == "CREDITO")
                .Include(v => v.Cliente)
                .Include(v => v.Abonos)
                .ToListAsync();

            var cartera = ventasCredito
                .GroupBy(v => v.ClienteId)
                .Select(g =>
                {
                    var primero = g.First();
                    decimal total = g.Sum(v => v.Total);
                    decimal abonado = g.Sum(v => v.Abonos.Sum(a => a.Monto));
                    decimal saldo = Math.Max(total - abonado, 0m);
                    var cli = primero.Cliente!;
                    return new CarteraCreditoDto
                    {
                        ClienteId = cli.Id,
                        Nombre = cli.Nombre,
                        Nit = cli.Nit,
                        Telefono = cli.Telefono,
                        LimiteCredito = cli.LimiteCredito,
                        SaldoPendiente = saldo,
                        Disponible = Math.Max(cli.LimiteCredito - saldo, 0m),
                        VentasCredito = g.Count(),
                        UltimaVenta = g.Max(v => v.FechaVenta)
                    };
                })
                .Where(c => c.SaldoPendiente > 0.009m)
                .OrderByDescending(c => c.SaldoPendiente)
                .ToList();

            return Ok(cartera);
        }

        // ============================================================
        //  VENTAS A CRÉDITO (por cliente o todas)
        // ============================================================

        [HttpGet("ventas")]
        public async Task<ActionResult<IEnumerable<CreditoVentaDto>>> GetVentasCredito([FromQuery] int? clienteId)
        {
            var query = _context.Ventas.AsNoTracking()
                .Include(v => v.Abonos)
                .Where(v => v.Estado == "CREDITO");

            if (clienteId.HasValue)
                query = query.Where(v => v.ClienteId == clienteId.Value);

            var ventas = await query.OrderByDescending(v => v.FechaVenta).ToListAsync();

            var resultado = ventas.Select(v =>
            {
                decimal abonado = v.Abonos.Sum(a => a.Monto);
                return new CreditoVentaDto
                {
                    VentaId = v.Id,
                    NumeroDocumento = v.NumeroDocumento,
                    FechaVenta = v.FechaVenta,
                    Total = v.Total,
                    Abonado = abonado,
                    Saldo = Math.Max(v.Total - abonado, 0m),
                    Estado = v.Estado
                };
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        //  HISTORIAL DE ABONOS
        // ============================================================

        [HttpGet("abonos")]
        public async Task<ActionResult<IEnumerable<AbonoCreditoDto>>> GetAbonos(
            [FromQuery] int? clienteId,
            [FromQuery] int? ventaId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 100)
        {
            var query = _context.CreditosAbonos.AsNoTracking()
                .Include(a => a.MetodoPago)
                .Include(a => a.Usuario)
                .Include(a => a.Venta).ThenInclude(v => v!.Cliente)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(a => a.Venta!.ClienteId == clienteId.Value);
            if (ventaId.HasValue)
                query = query.Where(a => a.VentaId == ventaId.Value);

            var abonos = await query
                .OrderByDescending(a => a.FechaAbono)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(a => new AbonoCreditoDto
                {
                    Id = a.Id,
                    VentaId = a.VentaId,
                    NumeroDocumento = a.Venta!.NumeroDocumento,
                    ClienteNombre = a.Venta.Cliente!.Nombre,
                    MetodoPagoId = a.MetodoPagoId,
                    MetodoPagoNombre = a.MetodoPago!.Nombre,
                    UsuarioNombre = $"{a.Usuario!.Nombre} {a.Usuario.Apellido}",
                    Monto = a.Monto,
                    Observacion = a.Observacion,
                    FechaAbono = a.FechaAbono
                })
                .ToListAsync();

            return Ok(abonos);
        }

        // ============================================================
        //  REGISTRAR ABONO (cobra la deuda de una venta a crédito)
        // ============================================================

        [HttpPost("abonos")]
        [TienePermiso("GestionCredito")]
        public async Task<ActionResult<AbonoCreditoDto>> RegistrarAbono([FromBody] RegistrarAbonoDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == model.VentaId);

            if (venta == null)
                return NotFound(new { mensaje = "Venta no encontrada" });

            if (venta.Estado != "CREDITO")
                return BadRequest(new { mensaje = "La venta no está a crédito (estado actual: " + venta.Estado + ")" });

            var metodoPago = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Id == model.MetodoPagoId && m.Activo);
            if (metodoPago == null)
                return BadRequest(new { mensaje = "Método de pago inválido o inactivo" });

            decimal abonado = await _context.CreditosAbonos
                .Where(a => a.VentaId == venta.Id)
                .SumAsync(a => (decimal?)a.Monto) ?? 0m;
            decimal saldo = Math.Max(venta.Total - abonado, 0m);

            if (model.Monto > saldo + 0.009m)
                return BadRequest(new { mensaje = $"El abono (Q{model.Monto:0.00}) excede el saldo pendiente (Q{saldo:0.00})" });

            // Sesión de caja abierta del usuario actual (para que el cobro entre al corte)
            var cajaAbierta = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Estado == "Abierta" && c.UsuarioId == CurrentUserId);
            var aperturaActual = cajaAbierta != null
                ? await _context.CajaAperturas
                    .Where(a => a.CajaId == cajaAbierta.Id)
                    .OrderByDescending(a => a.FechaApertura)
                    .FirstOrDefaultAsync()
                : null;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var abono = new AbonoCredito
                {
                    VentaId = venta.Id,
                    MetodoPagoId = metodoPago.Id,
                    UsuarioId = CurrentUserId,
                    CajaAperturaId = aperturaActual?.Id,
                    Monto = model.Monto,
                    Observacion = model.Observacion?.Trim(),
                    FechaAbono = DateTime.UtcNow
                };
                _context.CreditosAbonos.Add(abono);

                // Si el abono cubre el saldo, la venta queda saldada
                decimal nuevoSaldo = Math.Max(saldo - model.Monto, 0m);
                bool saldada = nuevoSaldo <= 0.009m;
                if (saldada)
                    venta.Estado = "PAGADA";

                _context.Auditorias.Add(new Auditoria
                {
                    UsuarioId = CurrentUserId,
                    Accion = saldada
                        ? $"Registró abono de Q{model.Monto:0.00} que salda la venta {venta.NumeroDocumento} (cliente: {venta.Cliente?.Nombre}) | Método: {metodoPago.Nombre}"
                        : $"Registró abono de Q{model.Monto:0.00} a la venta {venta.NumeroDocumento} (saldo restante: Q{nuevoSaldo:0.00}) | Método: {metodoPago.Nombre}",
                    TablaAfectada = "creditos_abonos",
                    RegistroId = venta.Id,
                    Fecha = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var dto = new AbonoCreditoDto
                {
                    Id = abono.Id,
                    VentaId = venta.Id,
                    NumeroDocumento = venta.NumeroDocumento,
                    ClienteNombre = venta.Cliente?.Nombre,
                    MetodoPagoId = metodoPago.Id,
                    MetodoPagoNombre = metodoPago.Nombre,
                    UsuarioNombre = User.Identity?.Name ?? "",
                    Monto = abono.Monto,
                    Observacion = abono.Observacion,
                    FechaAbono = abono.FechaAbono
                };

                return Ok(new
                {
                    mensaje = saldada
                        ? $"Abono registrado. La venta {venta.NumeroDocumento} quedó saldada."
                        : $"Abono registrado. Saldo restante: Q{nuevoSaldo:0.00}",
                    abono = dto,
                    saldoRestante = nuevoSaldo,
                    saldada
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
