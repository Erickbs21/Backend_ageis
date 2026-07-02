using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ApiAegis.Data;
using ApiAegis.DTOs;
using ApiAegis.Models;
using ApiAegis.Helpers;
using BCrypt.Net;

namespace ApiAegis.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public UsuariosController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [TienePermiso("GestionUsuarios")]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Correo = u.Correo,
                    NombreUsuario = u.NombreUsuario,
                    RolId = u.RolId,
                    RolNombre = u.Rol!.Nombre,
                    Activo = u.Activo,
                    UltimoAcceso = u.UltimoAcceso,
                    FechaCreacion = u.FechaCreacion
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        [TienePermiso("GestionUsuarios")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var dto = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                NombreUsuario = usuario.NombreUsuario,
                RolId = usuario.RolId,
                RolNombre = usuario.Rol!.Nombre,
                Activo = usuario.Activo,
                UltimoAcceso = usuario.UltimoAcceso,
                FechaCreacion = usuario.FechaCreacion
            };

            return Ok(dto);
        }

        [HttpPost]
        [TienePermiso("GestionUsuarios")]
        public async Task<ActionResult<UsuarioDto>> CrearUsuario([FromBody] CrearUsuarioDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existeUsuario = await _context.Usuarios.AnyAsync(u => u.NombreUsuario.ToLower() == model.NombreUsuario.ToLower());
            if (existeUsuario)
                return BadRequest(new { mensaje = "El nombre de usuario ya está registrado" });

            var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == model.Correo.ToLower());
            if (existeCorreo)
                return BadRequest(new { mensaje = "El correo ya está registrado" });

            var rol = await _context.Roles.FindAsync(model.RolId);
            if (rol == null)
                return BadRequest(new { mensaje = "El rol especificado no existe" });

            var nuevoUsuario = new Usuario
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Correo = model.Correo,
                NombreUsuario = model.NombreUsuario,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RolId = model.RolId,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(nuevoUsuario);

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Creó el usuario {nuevoUsuario.NombreUsuario}",
                TablaAfectada = "usuarios",
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            var dto = new UsuarioDto
            {
                Id = nuevoUsuario.Id,
                Nombre = nuevoUsuario.Nombre,
                Apellido = nuevoUsuario.Apellido,
                Correo = nuevoUsuario.Correo,
                NombreUsuario = nuevoUsuario.NombreUsuario,
                RolId = nuevoUsuario.RolId,
                RolNombre = rol.Nombre,
                Activo = nuevoUsuario.Activo,
                FechaCreacion = nuevoUsuario.FechaCreacion
            };

            return CreatedAtAction(nameof(GetUsuario), new { id = nuevoUsuario.Id }, dto);
        }

        [HttpPut("{id}")]
        [TienePermiso("GestionUsuarios")]
        public async Task<IActionResult> EditarUsuario(int id, [FromBody] EditarUsuarioDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (usuario.Correo.ToLower() != model.Correo.ToLower())
            {
                var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == model.Correo.ToLower() && u.Id != id);
                if (existeCorreo)
                    return BadRequest(new { mensaje = "El correo ya está registrado por otro usuario" });
            }

            var rol = await _context.Roles.FindAsync(model.RolId);
            if (rol == null)
                return BadRequest(new { mensaje = "El rol especificado no existe" });

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Correo = model.Correo;
            usuario.RolId = model.RolId;
            usuario.Activo = model.Activo;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Modificó el usuario {usuario.NombreUsuario}",
                TablaAfectada = "usuarios",
                RegistroId = usuario.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("reset-password")]
        [TienePermiso("GestionUsuarios")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NuevoPassword);
            usuario.RefreshToken = null; // Invalidar token de sesión actual

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Restableció contraseña para el usuario {usuario.NombreUsuario}",
                TablaAfectada = "usuarios",
                RegistroId = usuario.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña restablecida correctamente" });
        }
    }
}
