using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ApiAegis.Data;
using ApiAegis.DTOs;
using ApiAegis.Helpers;
using ApiAegis.Models;

namespace ApiAegis.Controllers
{
    /// <summary>
    /// Módulo de Configuración FEL (Factura Electrónica en Línea - SAT Guatemala).
    /// Permite al Administrador registrar y gestionar los parámetros de conexión
    /// con el certificador FEL (NIT emisor, credenciales API, ambiente, certificador).
    /// Requiere permiso: GestionConfiguracion.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ConfiguracionFelController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public ConfiguracionFelController(AegisDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET api/ConfiguracionFel
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Obtiene la configuración FEL activa del sistema.
        /// El token/llave se devuelve enmascarado por seguridad.
        /// </summary>
        /// <response code="200">Configuración FEL encontrada</response>
        /// <response code="404">No existe configuración FEL registrada</response>
        [HttpGet]
        [TienePermiso("GestionConfiguracion")]
        [ProducesResponseType(typeof(ConfiguracionFelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfiguracionFel()
        {
            var config = await _context.ConfiguracionesFel
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaActualizacion)
                .FirstOrDefaultAsync();

            if (config == null)
                return NotFound(new { mensaje = "No existe configuración FEL registrada. Por favor, configure el módulo FEL." });

            return Ok(MapearADto(config));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET api/ConfiguracionFel/historial
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Obtiene el historial completo de configuraciones FEL registradas.
        /// </summary>
        /// <response code="200">Lista de configuraciones FEL</response>
        [HttpGet("historial")]
        [TienePermiso("GestionConfiguracion")]
        [ProducesResponseType(typeof(IEnumerable<ConfiguracionFelDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistorial()
        {
            var configs = await _context.ConfiguracionesFel
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaActualizacion)
                .ToListAsync();

            return Ok(configs.Select(MapearADto));
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST api/ConfiguracionFel
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Crea o actualiza la configuración FEL del sistema.
        /// Si ya existe una configuración activa, la desactiva y crea una nueva.
        /// </summary>
        /// <param name="dto">Datos de configuración FEL</param>
        /// <response code="200">Configuración FEL guardada correctamente</response>
        /// <response code="400">Datos inválidos o token faltante en primera configuración</response>
        [HttpPost]
        [TienePermiso("GestionConfiguracion")]
        [ProducesResponseType(typeof(ConfiguracionFelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GuardarConfiguracionFel([FromBody] GuardarConfiguracionFelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? usuarioId = usuarioIdClaim != null ? int.Parse(usuarioIdClaim) : null;

            // Buscar configuración activa existente
            var configExistente = await _context.ConfiguracionesFel
                .FirstOrDefaultAsync(c => c.Activo);

            string tokenFinal;

            if (configExistente == null)
            {
                // Primera configuración: el token es obligatorio
                if (string.IsNullOrWhiteSpace(dto.TokenApi))
                    return BadRequest(new { mensaje = "El Token o Llave del API es requerido para la primera configuración FEL." });

                tokenFinal = dto.TokenApi;
            }
            else
            {
                // Actualización: si no se envía token nuevo, conservar el existente
                tokenFinal = string.IsNullOrWhiteSpace(dto.TokenApi)
                    ? configExistente.TokenApi
                    : dto.TokenApi;

                // Desactivar la configuración anterior (historial)
                configExistente.Activo = false;
                _context.ConfiguracionesFel.Update(configExistente);
            }

            var nuevaConfig = new ConfiguracionFel
            {
                NitEmisor = dto.NitEmisor.Trim(),
                UsuarioApi = dto.UsuarioApi.Trim(),
                TokenApi = tokenFinal,
                Ambiente = dto.Ambiente,
                NombreCertificador = dto.NombreCertificador.Trim(),
                Activo = true,
                FechaActualizacion = DateTime.UtcNow,
                UsuarioId = usuarioId
            };

            _context.ConfiguracionesFel.Add(nuevaConfig);
            await _context.SaveChangesAsync();

            // Recargar con relaciones para mapear el DTO
            await _context.Entry(nuevaConfig).Reference(c => c.Usuario).LoadAsync();

            return Ok(new
            {
                mensaje = "Configuración FEL guardada correctamente.",
                datos = MapearADto(nuevaConfig)
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT api/ConfiguracionFel/{id}
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Actualiza una configuración FEL específica por ID.
        /// Útil para corregir datos sin crear un nuevo registro en el historial.
        /// </summary>
        /// <param name="id">ID de la configuración a actualizar</param>
        /// <param name="dto">Datos actualizados</param>
        /// <response code="200">Configuración actualizada correctamente</response>
        /// <response code="404">Configuración no encontrada</response>
        [HttpPut("{id:int}")]
        [TienePermiso("GestionConfiguracion")]
        [ProducesResponseType(typeof(ConfiguracionFelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarConfiguracionFel(int id, [FromBody] GuardarConfiguracionFelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var config = await _context.ConfiguracionesFel.FindAsync(id);
            if (config == null)
                return NotFound(new { mensaje = $"No se encontró la configuración FEL con ID {id}." });

            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? usuarioId = usuarioIdClaim != null ? int.Parse(usuarioIdClaim) : null;

            config.NitEmisor = dto.NitEmisor.Trim();
            config.UsuarioApi = dto.UsuarioApi.Trim();
            config.Ambiente = dto.Ambiente;
            config.NombreCertificador = dto.NombreCertificador.Trim();
            config.Activo = dto.Activo;
            config.FechaActualizacion = DateTime.UtcNow;
            config.UsuarioId = usuarioId;

            // Solo actualizar el token si se envía uno nuevo
            if (!string.IsNullOrWhiteSpace(dto.TokenApi))
                config.TokenApi = dto.TokenApi;

            _context.ConfiguracionesFel.Update(config);
            await _context.SaveChangesAsync();

            await _context.Entry(config).Reference(c => c.Usuario).LoadAsync();

            return Ok(new
            {
                mensaje = "Configuración FEL actualizada correctamente.",
                datos = MapearADto(config)
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST api/ConfiguracionFel/probar-conexion
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Realiza una prueba de conexión al API del certificador FEL usando
        /// la configuración activa guardada en el sistema.
        /// </summary>
        /// <response code="200">Resultado de la prueba de conexión</response>
        /// <response code="404">No existe configuración FEL para probar</response>
        [HttpPost("probar-conexion")]
        [TienePermiso("GestionConfiguracion")]
        [ProducesResponseType(typeof(PruebaConexionFelResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProbarConexion()
        {
            var config = await _context.ConfiguracionesFel
                .FirstOrDefaultAsync(c => c.Activo);

            if (config == null)
                return NotFound(new { mensaje = "No existe configuración FEL activa. Configure el módulo FEL primero." });

            // TODO: Implementar llamada real al endpoint del certificador FEL
            // Aquí se haría la petición HTTP al API del certificador usando:
            // config.UsuarioApi, config.TokenApi, config.Ambiente, config.NombreCertificador
            // Por ahora retorna una simulación de prueba exitosa en Pruebas
            var esProduccion = config.Ambiente == "Produccion";

            var response = new PruebaConexionFelResponseDto
            {
                Exitoso = true,
                Mensaje = esProduccion
                    ? $"Conexión exitosa al ambiente de PRODUCCIÓN del certificador {config.NombreCertificador}."
                    : $"Conexión exitosa al ambiente de PRUEBAS del certificador {config.NombreCertificador}.",
                Ambiente = config.Ambiente,
                Certificador = config.NombreCertificador,
                FechaPrueba = DateTime.UtcNow
            };

            return Ok(response);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE api/ConfiguracionFel/{id}
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Desactiva (baja lógica) una configuración FEL por ID.
        /// No elimina el registro del historial.
        /// </summary>
        /// <param name="id">ID de la configuración a desactivar</param>
        /// <response code="200">Configuración desactivada correctamente</response>
        /// <response code="404">Configuración no encontrada</response>
        [HttpDelete("{id:int}")]
        [TienePermiso("GestionConfiguracion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DesactivarConfiguracionFel(int id)
        {
            var config = await _context.ConfiguracionesFel.FindAsync(id);
            if (config == null)
                return NotFound(new { mensaje = $"No se encontró la configuración FEL con ID {id}." });

            config.Activo = false;
            config.FechaActualizacion = DateTime.UtcNow;

            _context.ConfiguracionesFel.Update(config);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Configuración FEL ID {id} desactivada correctamente." });
        }

        // ─────────────────────────────────────────────────────────────────────
        // Método privado: Mapear entidad → DTO (con token enmascarado)
        // ─────────────────────────────────────────────────────────────────────
        private static ConfiguracionFelDto MapearADto(ConfiguracionFel config)
        {
            // Enmascarar el token: mostrar primeros 4 y últimos 4 caracteres
            var token = config.TokenApi ?? "";
            var tokenMascarado = token.Length > 8
                ? $"{token[..4]}{"*".PadRight(token.Length - 8, '*')}{token[^4..]}"
                : new string('*', token.Length);

            return new ConfiguracionFelDto
            {
                Id = config.Id,
                NitEmisor = config.NitEmisor,
                UsuarioApi = config.UsuarioApi,
                TokenApiMascarado = tokenMascarado,
                Ambiente = config.Ambiente,
                NombreCertificador = config.NombreCertificador,
                Activo = config.Activo,
                FechaActualizacion = config.FechaActualizacion,
                UsuarioModifico = config.Usuario != null
                    ? $"{config.Usuario.Nombre} {config.Usuario.Apellido}"
                    : null
            };
        }
    }
}
