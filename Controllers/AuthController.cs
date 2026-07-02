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
                .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == model.Usuario.ToLower() && u.Activo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
            {
                // Registrar auditoría de intento fallido
                var errorAudit = new Auditoria
                {
                    Accion = $"Intento fallido de inicio de sesión para el usuario: {model.Usuario}",
                    TablaAfectada = "usuarios",
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Fecha = DateTime.UtcNow
                };
                _context.Auditorias.Add(errorAudit);
                await _context.SaveChangesAsync();

                return Unauthorized(new { mensaje = "Credenciales incorrectas o usuario inactivo" });
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
