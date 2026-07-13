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
    public class CategoriasController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public CategoriasController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias()
        {
            var categorias = await _context.Categorias
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Activo = c.Activo
                })
                .ToListAsync();

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada" });

            return Ok(new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo
            });
        }

        [HttpPost]
        [TienePermiso("GestionProductos")]
        public async Task<ActionResult<CategoriaDto>> CrearCategoria([FromBody] CrearCategoriaDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == model.Nombre.ToLower());
            if (existe)
                return BadRequest(new { mensaje = "Ya existe una categoría con ese nombre" });

            var nueva = new Categoria
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Activo = true
            };

            _context.Categorias.Add(nueva);

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Creó categoría: {nueva.Nombre}",
                TablaAfectada = "categorias",
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            var dto = new CategoriaDto
            {
                Id = nueva.Id,
                Nombre = nueva.Nombre,
                Descripcion = nueva.Descripcion,
                Activo = nueva.Activo
            };

            return CreatedAtAction(nameof(GetCategoria), new { id = nueva.Id }, dto);
        }

        [HttpPut("{id}")]
        [TienePermiso("GestionProductos")]
        public async Task<IActionResult> EditarCategoria(int id, [FromBody] CrearCategoriaDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada" });

            if (categoria.Nombre.ToLower() != model.Nombre.ToLower())
            {
                var existe = await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == model.Nombre.ToLower() && c.Id != id);
                if (existe)
                    return BadRequest(new { mensaje = "Ya existe una categoría con ese nombre" });
            }

            categoria.Nombre = model.Nombre;
            categoria.Descripcion = model.Descripcion;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Modificó categoría: {categoria.Nombre}",
                TablaAfectada = "categorias",
                RegistroId = categoria.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [TienePermiso("GestionProductos")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada" });

            categoria.Activo = !categoria.Activo;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Cambió estado de categoría {categoria.Nombre} a {(categoria.Activo ? "Activo" : "Inactivo")}",
                TablaAfectada = "categorias",
                RegistroId = categoria.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Categoría {(categoria.Activo ? "activada" : "desactivada")} correctamente" });
        }
    }
}
