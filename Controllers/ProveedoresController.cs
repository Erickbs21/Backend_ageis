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
    public class ProveedoresController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public ProveedoresController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetProveedores()
        {
            var proveedores = await _context.Proveedores
                .Select(p => new ProveedorDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Nit = p.Nit,
                    Telefono = p.Telefono,
                    Correo = p.Correo,
                    Direccion = p.Direccion,
                    Activo = p.Activo
                })
                .ToListAsync();

            return Ok(proveedores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedorDto>> GetProveedor(int id)
        {
            var p = await _context.Proveedores.FindAsync(id);
            if (p == null)
                return NotFound(new { mensaje = "Proveedor no encontrado" });

            return Ok(new ProveedorDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Nit = p.Nit,
                Telefono = p.Telefono,
                Correo = p.Correo,
                Direccion = p.Direccion,
                Activo = p.Activo
            });
        }

        [HttpPost]
        [TienePermiso("GestionInventario")]
        public async Task<ActionResult<ProveedorDto>> CrearProveedor([FromBody] CrearProveedorDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Proveedores.AnyAsync(p => p.Nit == model.Nit);
            if (existe)
                return BadRequest(new { mensaje = "Ya existe un proveedor con ese NIT" });

            var nuevo = new Proveedor
            {
                Nombre = model.Nombre,
                Nit = model.Nit,
                Telefono = model.Telefono,
                Correo = model.Correo,
                Direccion = model.Direccion,
                Activo = true
            };

            _context.Proveedores.Add(nuevo);

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Creó proveedor: {nuevo.Nombre} (NIT: {nuevo.Nit})",
                TablaAfectada = "proveedores",
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            var dto = new ProveedorDto
            {
                Id = nuevo.Id,
                Nombre = nuevo.Nombre,
                Nit = nuevo.Nit,
                Telefono = nuevo.Telefono,
                Correo = nuevo.Correo,
                Direccion = nuevo.Direccion,
                Activo = nuevo.Activo
            };

            return CreatedAtAction(nameof(GetProveedor), new { id = nuevo.Id }, dto);
        }

        [HttpPut("{id}")]
        [TienePermiso("GestionInventario")]
        public async Task<IActionResult> EditarProveedor(int id, [FromBody] CrearProveedorDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var p = await _context.Proveedores.FindAsync(id);
            if (p == null)
                return NotFound(new { mensaje = "Proveedor no encontrado" });

            if (p.Nit != model.Nit)
            {
                var existe = await _context.Proveedores.AnyAsync(pr => pr.Nit == model.Nit && pr.Id != id);
                if (existe)
                    return BadRequest(new { mensaje = "Ya existe otro proveedor con ese NIT" });
            }

            p.Nombre = model.Nombre;
            p.Nit = model.Nit;
            p.Telefono = model.Telefono;
            p.Correo = model.Correo;
            p.Direccion = model.Direccion;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Modificó proveedor: {p.Nombre} (NIT: {p.Nit})",
                TablaAfectada = "proveedores",
                RegistroId = p.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [TienePermiso("GestionInventario")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var p = await _context.Proveedores.FindAsync(id);
            if (p == null)
                return NotFound(new { mensaje = "Proveedor no encontrado" });

            p.Activo = !p.Activo;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Cambió estado de proveedor {p.Nombre} a {(p.Activo ? "Activo" : "Inactivo")}",
                TablaAfectada = "proveedores",
                RegistroId = p.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Proveedor {(p.Activo ? "activado" : "desactivado")} correctamente" });
        }
    }
}
