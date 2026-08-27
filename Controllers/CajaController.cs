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
    public class CajaController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public CajaController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CajaDto>>> GetCajas()
        {
            var cajas = await _context.Cajas
                .Include(c => c.Usuario)
                .Select(c => new CajaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Estado = c.Estado,
                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario != null ? $"{c.Usuario.Nombre} {c.Usuario.Apellido}" : null
                })
                .ToListAsync();

            return Ok(cajas);
        }

        [HttpPost]
        [TienePermiso("GestionCaja")]
        public async Task<ActionResult<CajaDto>> CrearCaja([FromBody] CrearCajaDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var caja = new Caja
            {
                Nombre = model.Nombre,
                UsuarioId = model.UsuarioId,
                Estado = "Cerrada"
            };

            _context.Cajas.Add(caja);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCajas), new { id = caja.Id }, new CajaDto
            {
                Id = caja.Id,
                Nombre = caja.Nombre,
                Estado = caja.Estado,
                UsuarioId = caja.UsuarioId
            });
        }

        [HttpGet("estado/{cajaId}")]
        [TienePermiso("GestionCaja")]
        public async Task<ActionResult<CajaDetalleSesionDto>> ObtenerEstadoCaja(int cajaId)
        {
            var caja = await _context.Cajas.FindAsync(cajaId);
            if (caja == null)
                return NotFound(new { mensaje = "Caja no encontrada" });

            var respuesta = new CajaDetalleSesionDto
            {
                CajaId = caja.Id,
                CajaNombre = caja.Nombre,
                Estado = caja.Estado
            };

            if (caja.Estado == "Abierta")
            {
                // Buscar la última apertura
                var apertura = await _context.CajaAperturas
                    .Where(a => a.CajaId == cajaId)
                    .OrderByDescending(a => a.FechaApertura)
                    .Include(a => a.Usuario)
                    .FirstOrDefaultAsync();

                if (apertura != null)
                {
                    respuesta.UsuarioId = apertura.UsuarioId;
                    respuesta.UsuarioNombre = $"{apertura.Usuario!.Nombre} {apertura.Usuario.Apellido}";
                    respuesta.MontoInicial = apertura.MontoInicial;
                    respuesta.FechaApertura = apertura.FechaApertura;

                    // Calcular ventas registradas en efectivo (y otros medios si aplica) desde la apertura
                    // En este POS simple, asumimos ventas totales realizadas por el usuario o en general desde la apertura de caja.
                    var ventasTotal = await _context.Ventas
                        .Where(v => v.FechaVenta >= apertura.FechaApertura && v.Estado == "PAGADA")
                        .SumAsync(v => v.Total);

                    respuesta.VentasRegistradas = ventasTotal;
                    respuesta.MontoEsperado = apertura.MontoInicial + ventasTotal;
                }
            }

            return Ok(respuesta);
        }

        [HttpPost("apertura")]
        [TienePermiso("GestionCaja")]
        public async Task<IActionResult> AbrirCaja([FromBody] AperturaCajaDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var caja = await _context.Cajas.FindAsync(model.CajaId);
            if (caja == null)
                return NotFound(new { mensaje = "Caja no encontrada" });

            if (caja.Estado == "Abierta")
                return BadRequest(new { mensaje = "La caja ya se encuentra abierta" });

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var vendedorId = model.UsuarioId ?? currentUserId;

            var apertura = new CajaApertura
            {
                CajaId = model.CajaId,
                UsuarioId = vendedorId,
                MontoInicial = model.MontoInicial,
                FechaApertura = DateTime.UtcNow
            };

            caja.Estado = "Abierta";
            _context.CajaAperturas.Add(apertura);

            // Auditoría
            var audit = new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Apertura de caja {caja.Nombre} con monto inicial de {model.MontoInicial}",
                TablaAfectada = "cajas",
                RegistroId = caja.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Caja abierta correctamente", aperturaId = apertura.Id });
        }

        [HttpPost("cierre")]
        [TienePermiso("GestionCaja")]
        public async Task<IActionResult> CerrarCaja([FromBody] CierreCajaDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var caja = await _context.Cajas.FindAsync(model.CajaId);
            if (caja == null)
                return NotFound(new { mensaje = "Caja no encontrada" });

            if (caja.Estado == "Cerrada")
                return BadRequest(new { mensaje = "La caja ya se encuentra cerrada" });

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var cierre = new CajaCierre
            {
                CajaId = model.CajaId,
                UsuarioId = currentUserId,
                MontoFinal = model.MontoFinal,
                Notas = model.Notas,
                FechaCierre = DateTime.UtcNow
            };

            caja.Estado = "Cerrada";
            _context.CajaCierres.Add(cierre);

            // Auditoría
            var audit = new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Cierre de caja {caja.Nombre} con monto final de {model.MontoFinal}",
                TablaAfectada = "cajas",
                RegistroId = caja.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Caja cerrada correctamente", cierreId = cierre.Id });
        }
    }
}
