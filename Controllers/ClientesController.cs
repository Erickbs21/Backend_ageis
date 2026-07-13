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
    public class ClientesController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public ClientesController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var clientes = await _context.Clientes
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nit = c.Nit,
                    Dpi = c.Dpi,
                    Nombre = c.Nombre,
                    Direccion = c.Direccion,
                    Telefono = c.Telefono,
                    Correo = c.Correo,
                    CreditoHabilitado = c.CreditoHabilitado,
                    LimiteCredito = c.LimiteCredito,
                    Activo = c.Activo,
                    FechaCreacion = c.FechaCreacion
                })
                .ToListAsync();

            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var c = await _context.Clientes.FindAsync(id);
            if (c == null)
                return NotFound(new { mensaje = "Cliente no encontrado" });

            return Ok(new ClienteDto
            {
                Id = c.Id,
                Nit = c.Nit,
                Dpi = c.Dpi,
                Nombre = c.Nombre,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Correo = c.Correo,
                CreditoHabilitado = c.CreditoHabilitado,
                LimiteCredito = c.LimiteCredito,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion
            });
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> BuscarClientes([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { mensaje = "El término de búsqueda es requerido" });

            query = query.ToLower();

            var clientes = await _context.Clientes
                .Where(c => c.Nombre.ToLower().Contains(query) ||
                            c.Nit.ToLower().Contains(query) ||
                            (c.Dpi != null && c.Dpi.ToLower().Contains(query)))
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nit = c.Nit,
                    Dpi = c.Dpi,
                    Nombre = c.Nombre,
                    Direccion = c.Direccion,
                    Telefono = c.Telefono,
                    Correo = c.Correo,
                    CreditoHabilitado = c.CreditoHabilitado,
                    LimiteCredito = c.LimiteCredito,
                    Activo = c.Activo,
                    FechaCreacion = c.FechaCreacion
                })
                .ToListAsync();

            return Ok(clientes);
        }

        [HttpGet("{id}/historial")]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetHistorialCompras(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado" });

            var ventas = await _context.Ventas
                .Where(v => v.ClienteId == id)
                .OrderByDescending(v => v.FechaVenta)
                .Select(v => new VentaDto
                {
                    Id = v.Id,
                    NumeroDocumento = v.NumeroDocumento,
                    Total = v.Total,
                    Estado = v.Estado,
                    FechaVenta = v.FechaVenta,
                    TipoDocumento = v.TipoDocumento
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpPost]
        [TienePermiso("GestionClientes")]
        public async Task<ActionResult<ClienteDto>> CrearCliente([FromBody] CrearClienteDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Nit != "CF")
            {
                var existe = await _context.Clientes.AnyAsync(c => c.Nit == model.Nit);
                if (existe)
                    return BadRequest(new { mensaje = "Ya existe un cliente con ese NIT" });
            }

            var nuevo = new Cliente
            {
                Nit = model.Nit,
                Dpi = model.Dpi,
                Nombre = model.Nombre,
                Direccion = model.Direccion,
                Telefono = model.Telefono,
                Correo = model.Correo,
                CreditoHabilitado = model.CreditoHabilitado,
                LimiteCredito = model.LimiteCredito,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Clientes.Add(nuevo);

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Creó cliente: {nuevo.Nombre} (NIT: {nuevo.Nit})",
                TablaAfectada = "clientes",
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            var dto = new ClienteDto
            {
                Id = nuevo.Id,
                Nit = nuevo.Nit,
                Dpi = nuevo.Dpi,
                Nombre = nuevo.Nombre,
                Direccion = nuevo.Direccion,
                Telefono = nuevo.Telefono,
                Correo = nuevo.Correo,
                CreditoHabilitado = nuevo.CreditoHabilitado,
                LimiteCredito = nuevo.LimiteCredito,
                Activo = nuevo.Activo,
                FechaCreacion = nuevo.FechaCreacion
            };

            return CreatedAtAction(nameof(GetCliente), new { id = nuevo.Id }, dto);
        }

        [HttpPut("{id}")]
        [TienePermiso("GestionClientes")]
        public async Task<IActionResult> EditarCliente(int id, [FromBody] CrearClienteDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var c = await _context.Clientes.FindAsync(id);
            if (c == null)
                return NotFound(new { mensaje = "Cliente no encontrado" });

            if (model.Nit != "CF" && c.Nit != model.Nit)
            {
                var existe = await _context.Clientes.AnyAsync(cl => cl.Nit == model.Nit && cl.Id != id);
                if (existe)
                    return BadRequest(new { mensaje = "Ya existe otro cliente con ese NIT" });
            }

            c.Nit = model.Nit;
            c.Dpi = model.Dpi;
            c.Nombre = model.Nombre;
            c.Direccion = model.Direccion;
            c.Telefono = model.Telefono;
            c.Correo = model.Correo;
            c.CreditoHabilitado = model.CreditoHabilitado;
            c.LimiteCredito = model.LimiteCredito;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Modificó cliente: {c.Nombre} (NIT: {c.Nit})",
                TablaAfectada = "clientes",
                RegistroId = c.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [TienePermiso("GestionClientes")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var c = await _context.Clientes.FindAsync(id);
            if (c == null)
                return NotFound(new { mensaje = "Cliente no encontrado" });

            if (id == 1)
                return BadRequest(new { mensaje = "No se puede desactivar el cliente por defecto (Consumidor Final)" });

            c.Activo = !c.Activo;

            // Auditoría
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var audit = new Auditoria
            {
                UsuarioId = currentUserId != null ? int.Parse(currentUserId) : null,
                Accion = $"Cambió estado de cliente {c.Nombre} a {(c.Activo ? "Activo" : "Inactivo")}",
                TablaAfectada = "clientes",
                RegistroId = c.Id,
                Fecha = DateTime.UtcNow
            };
            _context.Auditorias.Add(audit);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Cliente {(c.Activo ? "activado" : "desactivado")} correctamente" });
        }
    }
}
