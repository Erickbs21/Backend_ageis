using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiAegis.Data;
using ApiAegis.DTOs;
using ApiAegis.Helpers;
using ApiAegis.Models;
using System.Security.Claims;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const int MaxIntentosFallidos = 5;
        private const int MinutosBloqueo = 15;

        private readonly AegisDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(AegisDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == model.Username.ToLower());

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (usuario == null)
            {
                _context.Auditorias.Add(new Auditoria
                {
                    Accion = $"Intento de sesión con usuario inexistente: {model.Username}",
                    TablaAfectada = "usuarios",
                    Ip = ip,
                    Fecha = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
            }

            if (!usuario.Activo)
            {
                _context.Auditorias.Add(new Auditoria
                {
                    UsuarioId = usuario.Id,
                    Accion = $"Intento de sesión con usuario desactivado: {usuario.NombreUsuario}",
                    TablaAfectada = "usuarios",
                    RegistroId = usuario.Id,
                    Ip = ip,
                    Fecha = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Unauthorized(new { mensaje = "Usuario desactivado. Contacte al administrador" });
            }

            if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.UtcNow)
            {
                var minutos = (int)Math.Ceiling((usuario.BloqueadoHasta.Value - DateTime.UtcNow).TotalMinutes);

                _context.Auditorias.Add(new Auditoria
                {
                    UsuarioId = usuario.Id,
                    Accion = $"Intento de sesión con cuenta bloqueada: {usuario.NombreUsuario} (desbloqueo en {minutos} min)",
                    TablaAfectada = "usuarios",
                    RegistroId = usuario.Id,
                    Ip = ip,
                    Fecha = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return StatusCode(403, new
                {
                    mensaje = $"Cuenta bloqueada temporalmente por intentos fallidos. Intente de nuevo en {minutos} minuto(s). Un administrador puede desbloquearla."
                });
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
            {
                usuario.IntentosFallidos++;
                var intento = usuario.IntentosFallidos;
                string mensajeIntentos;

                if (intento >= MaxIntentosFallidos)
                {
                    usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);
                    usuario.IntentosFallidos = 0;
                    mensajeIntentos = $"Cuenta bloqueada por {MaxIntentosFallidos} intentos fallidos. Espere {MinutosBloqueo} minutos o contacte al administrador.";
                }
                else
                {
                    var restantes = MaxIntentosFallidos - intento;
                    mensajeIntentos = $"Usuario o contraseña incorrectos. Intentos restantes: {restantes}";
                }

                _context.Auditorias.Add(new Auditoria
                {
                    UsuarioId = usuario.Id,
                    Accion = $"Intento fallido de inicio de sesión para {usuario.NombreUsuario} (intento {intento} de {MaxIntentosFallidos})",
                    TablaAfectada = "usuarios",
                    RegistroId = usuario.Id,
                    Ip = ip,
                    Fecha = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return Unauthorized(new { mensaje = mensajeIntentos });
            }

            // Obtener permisos del usuario a través de su Rol
            var permisos = await _context.RolPermisos
                .Where(rp => rp.RolId == usuario.RolId)
                .Include(rp => rp.Permiso)
                .Select(rp => rp.Permiso!.Nombre)
                .ToListAsync();

            // Generar Tokens
            var token = _jwtHelper.GenerarToken(usuario, permisos);
            var refreshToken = _jwtHelper.GenerarRefreshToken();

            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpira = DateTime.UtcNow.AddDays(7);
            usuario.UltimoAcceso = DateTime.UtcNow;
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;

            // Registrar auditoría de acceso exitoso
            var audit = new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = "Inicio de sesión exitoso",
                TablaAfectada = "usuarios",
                RegistroId = usuario.Id,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new LoginResponseDto
            {
                Id = usuario.Id,
                Token = token,
                RefreshToken = refreshToken,
                Usuario = usuario.NombreUsuario,
                Nombre = $"{usuario.Nombre} {usuario.Apellido}",
                Rol = usuario.Rol?.Nombre ?? "",
                Permisos = permisos
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ClaimsPrincipal? principal = null;
            try
            {
                principal = _jwtHelper.ObtenerPrincipalDeTokenExpirado(model.Token);
            }
            catch (Exception)
            {
                return BadRequest(new { mensaje = "Token inválido" });
            }

            if (principal == null)
            {
                return BadRequest(new { mensaje = "Token inválido" });
            }

            var username = principal.Identity?.Name;
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == username && u.Activo);

            if (usuario == null || usuario.RefreshToken != model.RefreshToken || usuario.RefreshTokenExpira <= DateTime.UtcNow)
            {
                return BadRequest(new { mensaje = "Refresh Token inválido o expirado" });
            }

            var permisos = await _context.RolPermisos
                .Where(rp => rp.RolId == usuario.RolId)
                .Include(rp => rp.Permiso)
                .Select(rp => rp.Permiso!.Nombre)
                .ToListAsync();

            var nuevoToken = _jwtHelper.GenerarToken(usuario, permisos);
            var nuevoRefreshToken = _jwtHelper.GenerarRefreshToken();

            usuario.RefreshToken = nuevoRefreshToken;
            usuario.RefreshTokenExpira = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = nuevoToken,
                refreshToken = nuevoRefreshToken
            });
        }
    }
}
