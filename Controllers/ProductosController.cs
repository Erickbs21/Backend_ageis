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
    public class ProductosController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public ProductosController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos([FromQuery] int pagina = 1, [FromQuery] int tamano = 100)
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    CodigoBarras = p.CodigoBarras,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria!.Nombre,
                    Marca = p.Marca,
                    Costo = p.Costo,
                    PrecioVenta = p.PrecioVenta,
                    PrecioMayoreo = p.PrecioMayoreo,
                    StockMinimo = p.StockMinimo,
                    StockActual = p.StockActual,
                    UsaCodigoBarras = p.UsaCodigoBarras,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var p = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            return Ok(new ProductoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                CodigoBarras = p.CodigoBarras,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria!.Nombre,
                Marca = p.Marca,
                Costo = p.Costo,
                PrecioVenta = p.PrecioVenta,
                PrecioMayoreo = p.PrecioMayoreo,
                StockMinimo = p.StockMinimo,
                StockActual = p.StockActual,
                UsaCodigoBarras = p.UsaCodigoBarras,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion
            });
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> BuscarProductos([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { mensaje = "El término de búsqueda es requerido" });

            var productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => EF.Functions.Like(p.Codigo, $"%{query}%") ||
                            EF.Functions.Like(p.CodigoBarras!, $"%{query}%") ||
                            EF.Functions.Like(p.Nombre, $"%{query}%") ||
                            EF.Functions.Like(p.Marca!, $"%{query}%"))
                .Take(20)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    CodigoBarras = p.CodigoBarras,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria!.Nombre,
                    Marca = p.Marca,
                    Costo = p.Costo,
                    PrecioVenta = p.PrecioVenta,
                    PrecioMayoreo = p.PrecioMayoreo,
                    StockMinimo = p.StockMinimo,
                    StockActual = p.StockActual,
                    UsaCodigoBarras = p.UsaCodigoBarras,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpPost]
        [TienePermiso("GestionProductos")]
        public async Task<ActionResult<ProductoDto>> CrearProducto([FromBody] CrearProductoDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existeCodigo = await _context.Productos.AnyAsync(p => p.Codigo.ToLower() == model.Codigo.ToLower());
            if (existeCodigo)
                return BadRequest(new { mensaje = "El código de producto ya existe" });

            if (!string.IsNullOrWhiteSpace(model.CodigoBarras))
            {
                var existeBarras = await _context.Productos.AnyAsync(p => p.CodigoBarras == model.CodigoBarras);
                if (existeBarras)
                    return BadRequest(new { mensaje = "El código de barras ya está asignado a otro producto" });
            }

            var categoria = await _context.Categorias.FindAsync(model.CategoriaId);
            if (categoria == null)
                return BadRequest(new { mensaje = "La categoría especificada no existe" });

            var nuevo = new Producto
            {
                Codigo = model.Codigo,
                CodigoBarras = model.CodigoBarras,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                CategoriaId = model.CategoriaId,
                Marca = model.Marca,
                Costo = model.Costo,
                PrecioVenta = model.PrecioVenta,
                PrecioMayoreo = model.PrecioMayoreo,
                StockMinimo = model.StockMinimo,
                StockActual = 0, // Se inicializa en 0, se carga con compras o ajustes
                UsaCodigoBarras = model.UsaCodigoBarras,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Productos.Add(nuevo);

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Creó producto: {nuevo.Nombre} (Código: {nuevo.Codigo})",
                TablaAfectada = "productos",
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            var dto = new ProductoDto
            {
                Id = nuevo.Id,
                Codigo = nuevo.Codigo,
                CodigoBarras = nuevo.CodigoBarras,
                Nombre = nuevo.Nombre,
                Descripcion = nuevo.Descripcion,
                CategoriaId = nuevo.CategoriaId,
                CategoriaNombre = categoria.Nombre,
                Marca = nuevo.Marca,
                Costo = nuevo.Costo,
                PrecioVenta = nuevo.PrecioVenta,
                PrecioMayoreo = nuevo.PrecioMayoreo,
                StockMinimo = nuevo.StockMinimo,
                StockActual = nuevo.StockActual,
                UsaCodigoBarras = nuevo.UsaCodigoBarras,
                Activo = nuevo.Activo,
                FechaCreacion = nuevo.FechaCreacion
            };

            return CreatedAtAction(nameof(GetProducto), new { id = nuevo.Id }, dto);
        }

        [HttpPut("{id}")]
        [TienePermiso("GestionProductos")]
        public async Task<IActionResult> EditarProducto(int id, [FromBody] CrearProductoDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var p = await _context.Productos.FindAsync(id);
            if (p == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            if (p.Codigo.ToLower() != model.Codigo.ToLower())
            {
                var existeCodigo = await _context.Productos.AnyAsync(pr => pr.Codigo.ToLower() == model.Codigo.ToLower() && pr.Id != id);
                if (existeCodigo)
                    return BadRequest(new { mensaje = "El código de producto ya existe" });
            }

            if (!string.IsNullOrWhiteSpace(model.CodigoBarras) && p.CodigoBarras != model.CodigoBarras)
            {
                var existeBarras = await _context.Productos.AnyAsync(pr => pr.CodigoBarras == model.CodigoBarras && pr.Id != id);
                if (existeBarras)
                    return BadRequest(new { mensaje = "El código de barras ya está asignado a otro producto" });
            }

            var categoria = await _context.Categorias.FindAsync(model.CategoriaId);
            if (categoria == null)
                return BadRequest(new { mensaje = "La categoría especificada no existe" });

            p.Codigo = model.Codigo;
            p.CodigoBarras = model.CodigoBarras;
            p.Nombre = model.Nombre;
            p.Descripcion = model.Descripcion;
            p.CategoriaId = model.CategoriaId;
            p.Marca = model.Marca;
            p.Costo = model.Costo;
            p.PrecioVenta = model.PrecioVenta;
            p.PrecioMayoreo = model.PrecioMayoreo;
            p.StockMinimo = model.StockMinimo;
            p.UsaCodigoBarras = model.UsaCodigoBarras;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Modificó producto: {p.Nombre} (ID: {p.Id})",
                TablaAfectada = "productos",
                RegistroId = p.Id,
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
            var p = await _context.Productos.FindAsync(id);
            if (p == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            p.Activo = !p.Activo;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Cambió estado de producto {p.Nombre} a {(p.Activo ? "Activo" : "Inactivo")}",
                TablaAfectada = "productos",
                RegistroId = p.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Producto {(p.Activo ? "activado" : "desactivada")} correctamente" });
        }
    }
}
