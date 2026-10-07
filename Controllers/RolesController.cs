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
    public class RolesController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public RolesController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [TienePermiso("GestionRoles")]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
        {
            var roles = await _context.Roles
                .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    Permisos = r.RolPermisos.Select(rp => new PermisoDto
                    {
                        Id = rp.Permiso!.Id,
                        Nombre = rp.Permiso.Nombre,
                        Descripcion = rp.Permiso.Descripcion
                    }).ToList()
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpGet("permisos")]
        [TienePermiso("GestionRoles")]
        public async Task<ActionResult<IEnumerable<PermisoDto>>> GetPermisos()
        {
            var permisos = await _context.Permisos
                .Select(p => new PermisoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion
                })
                .ToListAsync();

            return Ok(permisos);
        }

        [HttpPost("{id}/permisos")]
        [TienePermiso("GestionRoles")]
        public async Task<IActionResult> AsignarPermisos(int id, [FromBody] AsignarPermisosDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
                return NotFound(new { mensaje = "Rol no encontrado" });

            // Remover permisos antiguos
            var antiguosPermisos = await _context.RolPermisos
                .Where(rp => rp.RolId == id)
                .ToListAsync();
            var idsAnteriores = antiguosPermisos.Select(rp => rp.PermisoId).ToHashSet();
            _context.RolPermisos.RemoveRange(antiguosPermisos);

            // El proveedor MySQL no traduce colecciones primitivas: se valida en memoria
            var todosPermisos = await _context.Permisos.ToListAsync();
            var validPermisoIds = todosPermisos
                .Where(p => model.PermisosIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToList();

            // Agregar nuevos permisos
            foreach (var permisoId in validPermisoIds)
            {
                _context.RolPermisos.Add(new RolPermiso
                {
                    RolId = id,
                    PermisoId = permisoId
                });
            }

            // Auditoría con detalle de permisos agregados y retirados
            var idsNuevos = validPermisoIds.ToHashSet();
            var agregados = todosPermisos.Where(p => idsNuevos.Contains(p.Id) && !idsAnteriores.Contains(p.Id)).Select(p => p.Nombre).ToList();
            var retirados = todosPermisos.Where(p => idsAnteriores.Contains(p.Id) && !idsNuevos.Contains(p.Id)).Select(p => p.Nombre).ToList();

            var detalle = new List<string>();
            if (agregados.Count > 0) detalle.Add("agregados: " + string.Join(", ", agregados));
            if (retirados.Count > 0) detalle.Add("retirados: " + string.Join(", ", retirados));

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Modificó permisos del rol: {rol.Nombre}" +
                         (detalle.Count > 0 ? " | " + string.Join(" | ", detalle) : " | sin cambios"),
                TablaAfectada = "roles",
                RegistroId = rol.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Permisos asignados correctamente" });
        }
    }
}
