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

        private int CurrentUserId =>
            int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        private async Task<bool> EsAdminAsync(int userId)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            return usuario?.Rol?.Nombre == "Administrador";
        }

        // ============================================================
        //  CAJAS
        // ============================================================

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

        [HttpDelete("{id}")]
        [TienePermiso("GestionCaja")]
        public async Task<IActionResult> EliminarCaja(int id)
        {
            var caja = await _context.Cajas.FindAsync(id);
            if (caja == null)
                return NotFound(new { mensaje = "Caja no encontrada" });

            if (caja.Estado == "Abierta")
                return BadRequest(new { mensaje = "No se puede eliminar una caja abierta. Ciérrela primero." });

            var currentUserId = CurrentUserId;

            try
            {
                var aperturas = await _context.CajaAperturas.Where(a => a.CajaId == id).ToListAsync();
                var aperturaIds = aperturas.Select(a => a.Id).ToHashSet();

                // El proveedor MySQL no traduce colecciones primitivas: se filtra en memoria
                var movimientos = _context.CajaMovimientos.AsEnumerable()
                    .Where(m => aperturaIds.Contains(m.CajaAperturaId)).ToList();
                var cierres = _context.CajaCierres.AsEnumerable()
                    .Where(c => c.CajaId == id || (c.CajaAperturaId.HasValue && aperturaIds.Contains(c.CajaAperturaId.Value)))
                    .ToList();

                // Las ventas se conservan: solo se desvinculan de la sesión eliminada
                var ventas = _context.Ventas.AsEnumerable()
                    .Where(v => v.CajaAperturaId.HasValue && aperturaIds.Contains(v.CajaAperturaId.Value))
                    .ToList();
                foreach (var venta in ventas)
                    venta.CajaAperturaId = null;

                _context.CajaMovimientos.RemoveRange(movimientos);
                _context.CajaCierres.RemoveRange(cierres);
                _context.CajaAperturas.RemoveRange(aperturas);
                _context.Cajas.Remove(caja);

                _context.Auditorias.Add(new Auditoria
                {
                    UsuarioId = currentUserId,
                    Accion = $"Eliminó caja: {caja.Nombre} (ID: {caja.Id}) | " +
                             $"sesiones: {aperturas.Count}, cortes: {cierres.Count}, movimientos: {movimientos.Count}, ventas desvinculadas: {ventas.Count}",
                    TablaAfectada = "cajas",
                    RegistroId = caja.Id,
                    Fecha = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = "No fue posible eliminar la caja porque tiene datos relacionados. " +
                              "Verifique que no existan sesiones o cortes pendientes.",
                    error = ex.Message
                });
            }

            return Ok(new { mensaje = "Caja eliminada correctamente" });
        }

        /// <summary>
        /// Asignar/cambiar vendedor de una caja. Solo se permite cuando la caja está cerrada.
        /// </summary>
        [HttpPost("asignar")]
        [TienePermiso("GestionCaja")]
        public async Task<IActionResult> AsignarVendedor([FromBody] AsignarVendedorDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var caja = await _context.Cajas.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Id == model.CajaId);
            if (caja == null)
                return NotFound(new { mensaje = "Caja no encontrada" });

            if (caja.Estado == "Abierta")
                return BadRequest(new { mensaje = "No se puede cambiar el vendedor de una caja abierta. Cierre la caja antes de reasignarla." });

            var vendedor = await _context.Usuarios.FindAsync(model.UsuarioId);
            if (vendedor == null || !vendedor.Activo)
                return BadRequest(new { mensaje = "El vendedor seleccionado no existe o está inactivo" });

            var currentUserId = CurrentUserId;
            var anterior = caja.UsuarioId;

            caja.UsuarioId = model.UsuarioId;
            caja.Usuario = vendedor;

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Asignó el vendedor {vendedor.Nombre} {vendedor.Apellido} a la caja {caja.Nombre}" +
                         (anterior.HasValue ? $" (anterior: ID {anterior.Value})" : " (sin vendedor previo)"),
                TablaAfectada = "cajas",
                RegistroId = caja.Id,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Vendedor asignado a {caja.Nombre}",
                usuarioId = caja.UsuarioId,
                usuarioNombre = $"{vendedor.Nombre} {vendedor.Apellido}"
            });
        }

        // ============================================================
        //  APERTURA
        // ============================================================

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

            var currentUserId = CurrentUserId;
            var vendedorId = model.UsuarioId ?? currentUserId;

            // Impedir segunda sesión para el mismo vendedor
            var vendedorYaAbierto = await _context.Cajas
                .AnyAsync(c => c.Estado == "Abierta" && c.UsuarioId == vendedorId);
            if (vendedorYaAbierto)
                return BadRequest(new { mensaje = "El vendedor ya tiene una caja abierta. Cierre esa sesión antes de abrir otra." });

            var vendedorAbierto = await _context.CajaAperturas
                .AnyAsync(a => a.UsuarioId == vendedorId && a.Caja!.Estado == "Abierta");
            if (vendedorAbierto)
                return BadRequest(new { mensaje = "El vendedor ya tiene una caja abierta. Cierre esa sesión antes de abrir otra." });

            var apertura = new CajaApertura
            {
                CajaId = model.CajaId,
                UsuarioId = vendedorId,
                MontoInicial = model.MontoInicial,
                Observaciones = model.Observaciones,
                AsignadoPor = model.UsuarioId.HasValue && model.UsuarioId.Value != currentUserId ? currentUserId : null,
                FechaApertura = DateTime.UtcNow
            };

            caja.Estado = "Abierta";
            caja.UsuarioId = vendedorId;
            _context.CajaAperturas.Add(apertura);

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Apertura de caja {caja.Nombre} con monto inicial de {model.MontoInicial:C}" +
                         (string.IsNullOrWhiteSpace(model.Observaciones) ? "" : $" | Obs: {model.Observaciones}"),
                TablaAfectada = "cajas",
                RegistroId = caja.Id,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Caja abierta correctamente", aperturaId = apertura.Id });
        }

        // ============================================================
        //  ESTADO DE LA SESIÓN (para mostrar al usuario)
        // ============================================================

        [HttpGet("estado/{cajaId}")]
        [TienePermiso("GestionCaja")]
        public async Task<ActionResult<CajaDetalleSesionDto>> ObtenerEstadoCaja(int cajaId)
        {
            var caja = await _context.Cajas.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Id == cajaId);
            if (caja == null)
                return NotFound(new { mensaje = "Caja no encontrada" });

            var respuesta = new CajaDetalleSesionDto
            {
                CajaId = caja.Id,
                CajaNombre = caja.Nombre,
                Estado = caja.Estado,
                UsuarioId = caja.UsuarioId,
                UsuarioNombre = caja.Usuario != null ? $"{caja.Usuario.Nombre} {caja.Usuario.Apellido}" : null
            };

            var apertura = await _context.CajaAperturas
                .Include(a => a.AsignadoPorUsuario)
                .Where(a => a.CajaId == cajaId)
                .OrderByDescending(a => a.FechaApertura)
                .FirstOrDefaultAsync();

            if (apertura != null)
            {
                respuesta.CajaAperturaId = apertura.Id;
                respuesta.MontoInicial = apertura.MontoInicial;
                respuesta.FechaApertura = apertura.FechaApertura;
                respuesta.Observaciones = apertura.Observaciones;
                respuesta.AsignadoPor = apertura.AsignadoPor;
                respuesta.AsignadoPorNombre = apertura.AsignadoPorUsuario != null
                    ? $"{apertura.AsignadoPorUsuario.Nombre} {apertura.AsignadoPorUsuario.Apellido}"
                    : null;

                if (caja.Estado == "Abierta")
                {
                    var resumen = await CalcularResumenSesion(apertura.Id);
                    if (resumen != null)
                        ApplyResumenToDetalle(respuesta, resumen);
                }
            }

            return Ok(respuesta);
        }

        private static void ApplyResumenToDetalle(CajaDetalleSesionDto dto, CierreResumenDto resumen)
        {
            dto.VentasRegistradas = resumen.TotalVentas;
            dto.CantidadVentas = resumen.CantidadVentas;
            dto.VentasEfectivo = resumen.VentasEfectivo;
            dto.VentasTarjeta = resumen.VentasTarjeta;
            dto.VentasTransferencia = resumen.VentasTransferencia;
            dto.VentasCheque = resumen.VentasCheque;
            dto.VentasCredito = resumen.VentasCredito;
            dto.VentasMixto = resumen.VentasMixto;
            dto.VentasAnuladas = resumen.VentasAnuladas;
            dto.Devoluciones = resumen.Devoluciones;
            dto.EntradasEfectivo = resumen.EntradasEfectivo;
            dto.SalidasEfectivo = resumen.SalidasEfectivo;
            dto.EfectivoEsperado = resumen.EfectivoEsperado;
            dto.MontoEsperado = resumen.EfectivoEsperado;
            dto.MovimientosCount = resumen.MovimientosCount;
        }

        // ============================================================
        //  RESUMEN EN VIVO (vista previa antes del cierre)
        // ============================================================

        [HttpGet("resumen/{aperturaId}")]
        [TienePermiso("GestionCaja")]
        public async Task<ActionResult<CierreResumenDto>> ObtenerResumen(int aperturaId)
        {
            var resumen = await CalcularResumenSesion(aperturaId);
            if (resumen == null)
                return NotFound(new { mensaje = "Sesión de caja no encontrada" });

            return Ok(resumen);
        }

        // ============================================================
        //  MOVIMIENTOS DE CAJA
        // ============================================================

        [HttpPost("movimientos")]
        [TienePermiso("GestionCaja")]
        public async Task<ActionResult<CajaMovimientoDto>> CrearMovimiento([FromBody] CrearMovimientoDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var apertura = await _context.CajaAperturas
                .Include(a => a.Caja)
                .FirstOrDefaultAsync(a => a.Id == model.CajaAperturaId);
            if (apertura == null)
                return NotFound(new { mensaje = "Sesión de caja no encontrada" });

            if (apertura.Caja == null || apertura.Caja.Estado != "Abierta")
                return BadRequest(new { mensaje = "La caja no está abierta. No se pueden registrar movimientos." });

            var currentUserId = CurrentUserId;

            // Solo el vendedor asignado o un administrador pueden registrar movimientos
            var esAdmin = await EsAdminAsync(currentUserId);
            if (!esAdmin && apertura.UsuarioId != currentUserId)
                return BadRequest(new { mensaje = "Solo el vendedor asignado a la caja puede registrar movimientos en esta sesión." });

            var movimiento = new CajaMovimiento
            {
                CajaAperturaId = model.CajaAperturaId,
                UsuarioId = currentUserId,
                Tipo = model.Tipo.ToUpperInvariant(),
                Monto = model.Monto,
                Motivo = model.Motivo.Trim(),
                Observacion = string.IsNullOrWhiteSpace(model.Observacion) ? null : model.Observacion.Trim(),
                Fecha = DateTime.UtcNow
            };

            _context.CajaMovimientos.Add(movimiento);

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"{movimiento.Tipo} de caja por {movimiento.Monto:C} - Motivo: {movimiento.Motivo}",
                TablaAfectada = "caja_movimientos",
                RegistroId = movimiento.CajaAperturaId,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId);

            return Ok(new CajaMovimientoDto
            {
                Id = movimiento.Id,
                CajaAperturaId = movimiento.CajaAperturaId,
                UsuarioId = movimiento.UsuarioId,
                UsuarioNombre = usuario != null ? $"{usuario.Nombre} {usuario.Apellido}" : null,
                Tipo = movimiento.Tipo,
                Monto = movimiento.Monto,
                Motivo = movimiento.Motivo,
                Observacion = movimiento.Observacion,
                Fecha = movimiento.Fecha
            });
        }

        [HttpGet("movimientos/{aperturaId}")]
        [TienePermiso("GestionCaja")]
        public async Task<ActionResult<IEnumerable<CajaMovimientoDto>>> GetMovimientos(int aperturaId)
        {
            var movimientos = await _context.CajaMovimientos
                .AsNoTracking()
                .Include(m => m.Usuario)
                .Where(m => m.CajaAperturaId == aperturaId)
                .OrderByDescending(m => m.Fecha)
                .Select(m => new CajaMovimientoDto
                {
                    Id = m.Id,
                    CajaAperturaId = m.CajaAperturaId,
                    UsuarioId = m.UsuarioId,
                    UsuarioNombre = m.Usuario != null ? $"{m.Usuario.Nombre} {m.Usuario.Apellido}" : null,
                    Tipo = m.Tipo,
                    Monto = m.Monto,
                    Motivo = m.Motivo,
                    Observacion = m.Observacion,
                    Fecha = m.Fecha
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        [HttpDelete("movimientos/{id}")]
        [TienePermiso("GestionCaja")]
        public async Task<IActionResult> EliminarMovimiento(int id)
        {
            var currentUserId = CurrentUserId;

            if (!await EsAdminAsync(currentUserId))
                return Forbid();

            var movimiento = await _context.CajaMovimientos.FindAsync(id);
            if (movimiento == null)
                return NotFound(new { mensaje = "Movimiento no encontrado" });

            var apertura = await _context.CajaAperturas.Include(a => a.Caja)
                .FirstOrDefaultAsync(a => a.Id == movimiento.CajaAperturaId);
            if (apertura != null && apertura.Caja != null && apertura.Caja.Estado != "Abierta")
                return BadRequest(new { mensaje = "No se puede eliminar un movimiento de una sesión cerrada." });

            _context.CajaMovimientos.Remove(movimiento);

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Eliminó movimiento de caja (ID {movimiento.Id}): {movimiento.Tipo} {movimiento.Monto:C} - {movimiento.Motivo}",
                TablaAfectada = "caja_movimientos",
                RegistroId = movimiento.Id,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Movimiento eliminado correctamente" });
        }

        // ============================================================
        //  CIERRE / CORTE DE CAJA
        // ============================================================

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

            var currentUserId = CurrentUserId;

            var apertura = await _context.CajaAperturas
                .Where(a => a.CajaId == model.CajaId)
                .OrderByDescending(a => a.FechaApertura)
                .FirstOrDefaultAsync();
            if (apertura == null)
                return BadRequest(new { mensaje = "No existe una apertura de caja para esta sesión." });

            var esDuenio = apertura.UsuarioId == currentUserId;
            var esAdmin = await EsAdminAsync(currentUserId);

            // Solo el dueño de la sesión o un administrador pueden cerrarla
            if (!esDuenio && !esAdmin)
                return BadRequest(new { mensaje = "Solo el vendedor asignado a la caja o un administrador pueden cerrar esta sesión." });

            // Calcular resumen autoritativo desde la base de datos
            var resumen = await CalcularResumenSesion(apertura.Id);
            if (resumen == null)
                return BadRequest(new { mensaje = "No se pudo calcular el resumen de la sesión." });

            var efectivoContado = model.EfectivoContado != 0 ? model.EfectivoContado : model.TotalConteo;
            var diferencia = Math.Round(efectivoContado - resumen.EfectivoEsperado, 2);
            var estadoCorte = Math.Abs(diferencia) <= 0.009m ? "CUADRADO" : diferencia < 0 ? "FALTANTE" : "SOBRANTE";
            var cierreAdministrativo = !esDuenio;

            // Cualquier diferencia (faltante o sobrante) y todo cierre administrativo exigen motivo
            if (cierreAdministrativo && string.IsNullOrWhiteSpace(model.Notas))
                return BadRequest(new { mensaje = "Debe indicar el motivo del cierre administrativo" });

            if (estadoCorte != "CUADRADO" && string.IsNullOrWhiteSpace(model.Notas))
                return BadRequest(new { mensaje = $"Debe indicar el motivo del corte ({estadoCorte})" });

            var dueno = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == apertura.UsuarioId);
            var duenoNombre = dueno != null ? $"{dueno.Nombre} {dueno.Apellido}" : "—";

            var cierre = new CajaCierre
            {
                CajaAperturaId = apertura.Id,
                CajaId = model.CajaId,
                UsuarioId = currentUserId,
                MontoFinal = efectivoContado,
                FondoInicial = resumen.FondoInicial,
                TotalVentas = resumen.TotalVentas,
                CantidadVentas = resumen.CantidadVentas,
                VentasEfectivo = resumen.VentasEfectivo,
                VentasTarjeta = resumen.VentasTarjeta,
                VentasTransferencia = resumen.VentasTransferencia,
                VentasCheque = resumen.VentasCheque,
                VentasCredito = resumen.VentasCredito,
                VentasMixto = resumen.VentasMixto,
                VentasAnuladas = resumen.VentasAnuladas,
                Devoluciones = resumen.Devoluciones,
                EntradasEfectivo = resumen.EntradasEfectivo,
                SalidasEfectivo = resumen.SalidasEfectivo,
                EfectivoEsperado = resumen.EfectivoEsperado,
                EfectivoContado = efectivoContado,
                Diferencia = diferencia,
                EstadoCorte = estadoCorte,
                BilletesQ200 = model.BilletesQ200,
                BilletesQ100 = model.BilletesQ100,
                BilletesQ50 = model.BilletesQ50,
                BilletesQ20 = model.BilletesQ20,
                BilletesQ10 = model.BilletesQ10,
                BilletesQ5 = model.BilletesQ5,
                Monedas = model.Monedas,
                TotalConteo = model.TotalConteo,
                Notas = string.IsNullOrWhiteSpace(model.Notas) ? null : model.Notas.Trim(),
                TipoCorte = cierreAdministrativo ? "ADMINISTRATIVO" : "NORMAL",
                FechaCierre = DateTime.UtcNow
            };

            caja.Estado = "Cerrada";
            _context.CajaCierres.Add(cierre);

            var motivo = string.IsNullOrWhiteSpace(model.Notas) ? "" : $" | Motivo: {model.Notas.Trim()}";
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Cierre {(cierreAdministrativo ? "ADMINISTRATIVO" : "NORMAL")} de caja {caja.Nombre}" +
                         $" | Sesión de: {duenoNombre} | Esperado: {resumen.EfectivoEsperado:C} | Contado: {efectivoContado:C}" +
                         $" | {estadoCorte} | Diferencia: {diferencia:C}{motivo}",
                TablaAfectada = "caja_cierres",
                RegistroId = caja.Id,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Caja cerrada correctamente",
                cierreId = cierre.Id,
                estado = estadoCorte,
                diferencia
            });
        }

        // ============================================================
        //  HISTORIAL DE SESIONES / CORTES
        // ============================================================

        [HttpGet("historial")]
        [TienePermiso("GestionCaja")]
        public async Task<ActionResult<IEnumerable<CorteCajaDto>>> GetHistorial(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int? usuarioId,
            [FromQuery] int? cajaId,
            [FromQuery] string? estado,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 50)
        {
            var query = _context.CajaCierres
                .AsNoTracking()
                .Include(c => c.Caja)
                .Include(c => c.CajaApertura)
                    .ThenInclude(a => a!.Usuario)
                .Include(c => c.Usuario)
                .AsQueryable();

            // Las fechas llegan como día calendario de Guatemala (UTC-6, sin horario de verano)
            var offset = new TimeSpan(-6, 0, 0);
            if (desde.HasValue)
            {
                var d = desde.Value.Date - offset;
                query = query.Where(c => c.FechaCierre >= d);
            }
            if (hasta.HasValue)
            {
                var h = hasta.Value.Date.AddDays(1) - offset;
                query = query.Where(c => c.FechaCierre < h);
            }
            if (usuarioId.HasValue)
                query = query.Where(c => c.CajaApertura!.UsuarioId == usuarioId.Value);
            if (cajaId.HasValue)
                query = query.Where(c => c.CajaId == cajaId.Value);
            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(c => c.EstadoCorte == estado.ToUpperInvariant());

            var total = await query.CountAsync();

            var cortes = await query
                .OrderByDescending(c => c.FechaCierre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(c => new CorteCajaDto
                {
                    Id = c.Id,
                    CajaId = c.CajaId,
                    CajaNombre = c.Caja!.Nombre,
                    CajaAperturaId = c.CajaAperturaId,
                    FechaApertura = c.CajaApertura != null ? c.CajaApertura.FechaApertura : (DateTime?)null,
                    FechaCierre = c.FechaCierre,
                    AbiertoPor = c.CajaApertura != null && c.CajaApertura.Usuario != null
                        ? $"{c.CajaApertura.Usuario.Nombre} {c.CajaApertura.Usuario.Apellido}" : null,
                    CerradoPor = c.Usuario != null ? $"{c.Usuario.Nombre} {c.Usuario.Apellido}" : null,
                    FondoInicial = c.FondoInicial,
                    TotalVentas = c.TotalVentas,
                    CantidadVentas = c.CantidadVentas,
                    VentasEfectivo = c.VentasEfectivo,
                    VentasTarjeta = c.VentasTarjeta,
                    VentasTransferencia = c.VentasTransferencia,
                    VentasCheque = c.VentasCheque,
                    VentasCredito = c.VentasCredito,
                    VentasMixto = c.VentasMixto,
                    VentasAnuladas = c.VentasAnuladas,
                    Devoluciones = c.Devoluciones,
                    EntradasEfectivo = c.EntradasEfectivo,
                    SalidasEfectivo = c.SalidasEfectivo,
                    EfectivoEsperado = c.EfectivoEsperado,
                    EfectivoContado = c.EfectivoContado,
                    Diferencia = c.Diferencia,
                    EstadoCorte = c.EstadoCorte,
                    Notas = c.Notas,
                    BilletesQ200 = c.BilletesQ200,
                    BilletesQ100 = c.BilletesQ100,
                    BilletesQ50 = c.BilletesQ50,
                    BilletesQ20 = c.BilletesQ20,
                    BilletesQ10 = c.BilletesQ10,
                    BilletesQ5 = c.BilletesQ5,
                    Monedas = c.Monedas,
                    TotalConteo = c.TotalConteo,
                    TipoCorte = c.TipoCorte,
                    MovimientosCount = 0
                })
                .ToListAsync();

            foreach (var corte in cortes)
            {
                corte.MovimientosCount = await _context.CajaMovimientos
                    .CountAsync(m => m.CajaAperturaId == corte.CajaAperturaId);
            }

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(cortes);
        }

        [HttpGet("historial/{cierreId}")]
        [TienePermiso("GestionCaja")]
        public async Task<ActionResult<object>> GetDetalleCorte(int cierreId)
        {
            var cierre = await _context.CajaCierres
                .AsNoTracking()
                .Include(c => c.Caja)
                .Include(c => c.CajaApertura).ThenInclude(a => a!.Usuario)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == cierreId);

            if (cierre == null)
                return NotFound(new { mensaje = "Corte no encontrado" });

            var movimientos = await _context.CajaMovimientos
                .AsNoTracking()
                .Include(m => m.Usuario)
                .Where(m => m.CajaAperturaId == cierre.CajaAperturaId)
                .OrderBy(m => m.Fecha)
                .Select(m => new CajaMovimientoDto
                {
                    Id = m.Id,
                    CajaAperturaId = m.CajaAperturaId,
                    UsuarioId = m.UsuarioId,
                    UsuarioNombre = m.Usuario != null ? $"{m.Usuario.Nombre} {m.Usuario.Apellido}" : null,
                    Tipo = m.Tipo,
                    Monto = m.Monto,
                    Motivo = m.Motivo,
                    Observacion = m.Observacion,
                    Fecha = m.Fecha
                })
                .ToListAsync();

            var ventas = await GetVentasSesion(cierre.CajaAperturaId ?? 0);
            var ventasDto = ventas.Select(v => new
            {
                v.Id,
                v.NumeroDocumento,
                v.NombreCliente,
                v.Estado,
                v.Total,
                v.FechaVenta,
                Metodo = v.MetodoPago != null ? v.MetodoPago.Nombre : "-",
                Pagos = v.Pagos.Select(p => new
                {
                    p.MetodoPagoId,
                    Metodo = p.MetodoPago != null ? p.MetodoPago.Nombre : "-",
                    p.Monto
                }).ToList()
            }).OrderBy(v => v.FechaVenta).ToList();

            var corteDto = new CorteCajaDto
            {
                Id = cierre.Id,
                CajaId = cierre.CajaId,
                CajaNombre = cierre.Caja?.Nombre ?? "",
                CajaAperturaId = cierre.CajaAperturaId,
                FechaApertura = cierre.CajaApertura?.FechaApertura,
                FechaCierre = cierre.FechaCierre,
                AbiertoPor = cierre.CajaApertura?.Usuario != null
                    ? $"{cierre.CajaApertura.Usuario.Nombre} {cierre.CajaApertura.Usuario.Apellido}" : null,
                CerradoPor = cierre.Usuario != null ? $"{cierre.Usuario.Nombre} {cierre.Usuario.Apellido}" : null,
                FondoInicial = cierre.FondoInicial,
                TotalVentas = cierre.TotalVentas,
                CantidadVentas = cierre.CantidadVentas,
                VentasEfectivo = cierre.VentasEfectivo,
                VentasTarjeta = cierre.VentasTarjeta,
                VentasTransferencia = cierre.VentasTransferencia,
                VentasCheque = cierre.VentasCheque,
                VentasCredito = cierre.VentasCredito,
                VentasMixto = cierre.VentasMixto,
                VentasAnuladas = cierre.VentasAnuladas,
                Devoluciones = cierre.Devoluciones,
                EntradasEfectivo = cierre.EntradasEfectivo,
                SalidasEfectivo = cierre.SalidasEfectivo,
                EfectivoEsperado = cierre.EfectivoEsperado,
                EfectivoContado = cierre.EfectivoContado,
                Diferencia = cierre.Diferencia,
                EstadoCorte = cierre.EstadoCorte,
                Notas = cierre.Notas,
                BilletesQ200 = cierre.BilletesQ200,
                BilletesQ100 = cierre.BilletesQ100,
                BilletesQ50 = cierre.BilletesQ50,
                BilletesQ20 = cierre.BilletesQ20,
                BilletesQ10 = cierre.BilletesQ10,
                BilletesQ5 = cierre.BilletesQ5,
                Monedas = cierre.Monedas,
                TotalConteo = cierre.TotalConteo,
                TipoCorte = cierre.TipoCorte,
                MovimientosCount = movimientos.Count
            };

            return Ok(new
            {
                corte = corteDto,
                movimientos,
                ventas = ventasDto
            });
        }

        // ============================================================
        //  HELPERS DE CÁLCULO
        // ============================================================

        /// <summary>
        /// Ventas de la sesión: usa CajaAperturaId (nuevas) y respaldo por
        /// usuario + rango de fechas para ventas antiguas sin vínculo.
        /// </summary>
        private async Task<List<Venta>> GetVentasSesion(int aperturaId)
        {
            var apertura = await _context.CajaAperturas.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == aperturaId);
            if (apertura == null)
                return new List<Venta>();

            var cierre = await _context.CajaCierres.AsNoTracking()
                .Where(c => c.CajaAperturaId == aperturaId)
                .OrderByDescending(c => c.FechaCierre)
                .FirstOrDefaultAsync();

            var limite = cierre != null ? cierre.FechaCierre : DateTime.UtcNow;

            return await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Pagos).ThenInclude(p => p.MetodoPago)
                .Include(v => v.MetodoPago)
                .Where(v =>
                    v.CajaAperturaId == aperturaId ||
                    (v.CajaAperturaId == null &&
                     v.UsuarioId == apertura.UsuarioId &&
                     v.FechaVenta >= apertura.FechaApertura &&
                     v.FechaVenta <= limite))
                .ToListAsync();
        }

        private async Task<CierreResumenDto?> CalcularResumenSesion(int aperturaId)
        {
            var apertura = await _context.CajaAperturas
                .AsNoTracking()
                .Include(a => a.Caja)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == aperturaId);
            if (apertura == null)
                return null;

            var ventas = await GetVentasSesion(aperturaId);
            var movimientos = await _context.CajaMovimientos.AsNoTracking()
                .Where(m => m.CajaAperturaId == aperturaId)
                .ToListAsync();

            var pagadas = ventas.Where(v => v.Estado == "PAGADA").ToList();
            var anuladas = ventas.Where(v => v.Estado == "ANULADA").ToList();

            decimal MontoPorMetodo(IEnumerable<Venta> lista, string nombre) =>
                lista.Sum(v => v.Pagos
                    .Where(p => p.MetodoPago != null && p.MetodoPago.Nombre == nombre)
                    .Sum(p => p.Monto));

            var totalVentas = pagadas.Sum(v => v.Total);
            var efectivo = MontoPorMetodo(pagadas, "Efectivo");
            var tarjeta = MontoPorMetodo(pagadas, "Tarjeta");
            var transferencia = MontoPorMetodo(pagadas, "Transferencia");
            var cheque = MontoPorMetodo(pagadas, "Cheque");
            var credito = MontoPorMetodo(pagadas, "Crédito");
            var mixto = pagadas.Where(v => v.MetodoPagoId == 6).Sum(v => v.Total);
            var anuladasTotal = anuladas.Sum(v => v.Total);
            var devoluciones = MontoPorMetodo(anuladas, "Efectivo");

            // Devoluciones parciales registradas sobre las ventas pagadas de esta sesión.
            // Se devuelve por el mismo método de pago, por lo que sólo descuenta la proporción en efectivo.
            var idsPagadas = pagadas.Select(v => v.Id).ToHashSet();
            if (idsPagadas.Count > 0)
            {
                var todas = await _context.Devoluciones.AsNoTracking()
                    .Select(x => new { x.VentaId, x.MontoDevuelto })
                    .ToListAsync();

                var infoVentas = pagadas.ToDictionary(
                    v => v.Id,
                    v => new { Efectivo = MontoPorMetodo(new[] { v }, "Efectivo"), Total = v.Total });

                devoluciones += todas
                    .Where(x => idsPagadas.Contains(x.VentaId))
                    .Sum(x =>
                        infoVentas.TryGetValue(x.VentaId, out var info) && info.Total > 0
                            ? Math.Round(x.MontoDevuelto * (info.Efectivo / info.Total), 2)
                            : 0m);
            }
            var entradas = movimientos.Where(m => m.Tipo == "ENTRADA").Sum(m => m.Monto);
            var salidas = movimientos.Where(m => m.Tipo == "SALIDA").Sum(m => m.Monto);

            var efectivoEsperado = apertura.MontoInicial + efectivo + entradas - salidas - devoluciones;

            return new CierreResumenDto
            {
                CajaId = apertura.CajaId,
                CajaAperturaId = apertura.Id,
                CajaNombre = apertura.Caja?.Nombre ?? "",
                VendedorNombre = apertura.Usuario != null
                    ? $"{apertura.Usuario.Nombre} {apertura.Usuario.Apellido}" : null,
                FechaApertura = apertura.FechaApertura,
                FondoInicial = apertura.MontoInicial,
                TotalVentas = totalVentas,
                CantidadVentas = pagadas.Count,
                VentasEfectivo = efectivo,
                VentasTarjeta = tarjeta,
                VentasTransferencia = transferencia,
                VentasCheque = cheque,
                VentasCredito = credito,
                VentasMixto = mixto,
                VentasAnuladas = anuladasTotal,
                Devoluciones = devoluciones,
                EntradasEfectivo = entradas,
                SalidasEfectivo = salidas,
                EfectivoEsperado = efectivoEsperado,
                MovimientosCount = movimientos.Count
            };
        }
    }
}
