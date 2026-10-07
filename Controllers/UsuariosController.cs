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

            var rolAnterior = await _context.Roles
                .Where(r => r.Id == usuario.RolId)
                .Select(r => r.Nombre)
                .FirstOrDefaultAsync();
            var activoAnterior = usuario.Activo;

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Correo = model.Correo;
            usuario.RolId = model.RolId;
            usuario.Activo = model.Activo;

            if (!activoAnterior && model.Activo)
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = null;
            }

            // Auditoría con detalle de cambios
            var cambios = new List<string>();
            if (rolAnterior != rol.Nombre) cambios.Add($"rol: {rolAnterior} -> {rol.Nombre}");
            if (activoAnterior != model.Activo) cambios.Add($"estado: {(activoAnterior ? "Activo" : "Inactivo")} -> {(model.Activo ? "Activo" : "Inactivo")}");

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Modificó el usuario {usuario.NombreUsuario}" +
                         (cambios.Count > 0 ? " | " + string.Join(", ", cambios) : ""),
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
            usuario.IntentosFallidos = 0; // Desbloqueo por restablecimiento de contraseña
            usuario.BloqueadoHasta = null;

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

        // La eliminación física rompería la trazabilidad (ventas, cajas, auditorías),
        // por lo que este endpoint desactiva el usuario y conserva su historial.
        [HttpDelete("{id}")]
        [TienePermiso("GestionUsuarios")]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (!usuario.Activo)
                return BadRequest(new { mensaje = "El usuario ya se encuentra desactivado" });

            var currentUserId = ObtenerUsuarioActualId();

            if (currentUserId == id)
                return BadRequest(new { mensaje = "No puede desactivar su propia cuenta" });

            // Guardia: siempre debe quedar al menos un administrador activo
            if (usuario.Rol?.Nombre == "Administrador")
            {
                var adminsActivos = await _context.Usuarios
                    .CountAsync(u => u.Activo && u.Rol!.Nombre == "Administrador" && u.Id != id);
                if (adminsActivos == 0)
                    return BadRequest(new { mensaje = "No se puede desactivar: debe existir al menos un administrador activo" });
            }

            usuario.Activo = false;
            usuario.RefreshToken = null;
            usuario.BloqueadoHasta = null;
            usuario.IntentosFallidos = 0;

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Desactivó el usuario {usuario.NombreUsuario} (rol: {usuario.Rol?.Nombre})",
                TablaAfectada = "usuarios",
                RegistroId = usuario.Id,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Usuario {usuario.NombreUsuario} desactivado. Su historial se conserva." });
        }

        [HttpPost("{id}/activar")]
        [TienePermiso("GestionUsuarios")]
        public async Task<IActionResult> ActivarUsuario(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (usuario.Activo)
                return BadRequest(new { mensaje = "El usuario ya se encuentra activo" });

            var currentUserId = ObtenerUsuarioActualId();

            usuario.Activo = true;
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,
                Accion = $"Activó el usuario {usuario.NombreUsuario} (rol: {usuario.Rol?.Nombre})",
                TablaAfectada = "usuarios",
                RegistroId = usuario.Id,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Usuario {usuario.NombreUsuario} activado" });
        }

        private int? ObtenerUsuarioActualId()
        {
            var valor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return valor != null && int.TryParse(valor, out var id) ? id : null;
        }
    }
}
