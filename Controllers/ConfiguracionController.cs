using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApiAegis.Data;
using ApiAegis.Helpers;
using ApiAegis.Models;
using System.Text.Json;

namespace ApiAegis.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracionController : ControllerBase
    {
        private readonly AegisDbContext _context;

        // Valores por defecto: solo se usan cuando la clave no existe en la base de datos
        private static readonly Dictionary<string, object> _valoresPorDefecto = new()
        {
            ["nombreEmpresa"] = "AEGIS POS",
            ["nit"] = "12345678-9",
            ["telefono"] = "7777-8888",
            ["direccion"] = "Ciudad de Guatemala, Guatemala",
            ["moneda"] = "GTQ",
            ["simboloMoneda"] = "Q",
            ["impuestoPorcentaje"] = 12.0,
            ["tema"] = "dark"
        };

        public ConfiguracionController(AegisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfiguracion()
        {
            return Ok(await CargarConfiguracionAsync());
        }

        [HttpPost]
        [HttpPut]
        [TienePermiso("GestionConfiguracion")]
        public async Task<IActionResult> GuardarConfiguracion([FromBody] Dictionary<string, JsonElement> nuevaConfiguracion)
        {
            if (nuevaConfiguracion == null || nuevaConfiguracion.Count == 0)
                return BadRequest(new { mensaje = "Configuración inválida" });

            var existentes = await _context.Configuraciones.ToListAsync();
            var cambios = new List<string>();

            foreach (var kvp in nuevaConfiguracion)
            {
                var valor = ConvertirValor(kvp.Value);
                var fila = existentes.FirstOrDefault(c => c.Clave == kvp.Key);

                if (fila == null)
                {
                    _context.Configuraciones.Add(new Configuracion { Clave = kvp.Key, Valor = valor });
                    cambios.Add($"{kvp.Key}: (nuevo) -> {Resumir(valor)}");
                }
                else if (fila.Valor != valor)
                {
                    cambios.Add($"{kvp.Key}: {Resumir(fila.Valor)} -> {Resumir(valor)}");
                    fila.Valor = valor;
                }
            }

            if (cambios.Count > 0)
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _context.Auditorias.Add(new Auditoria
                {
                    UsuarioId = currentUserId != null && int.TryParse(currentUserId, out var uid) ? uid : null,
                    Accion = "Modificó la configuración del sistema | " + string.Join(", ", cambios),
                    TablaAfectada = "configuraciones",
                    Fecha = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Configuración guardada correctamente", datos = await CargarConfiguracionAsync() });
        }

        private async Task<Dictionary<string, object>> CargarConfiguracionAsync()
        {
            var configuracion = new Dictionary<string, object>();

            foreach (var kvp in _valoresPorDefecto)
                configuracion[kvp.Key] = kvp.Value;

            var filas = await _context.Configuraciones.ToListAsync();
            foreach (var fila in filas)
                configuracion[fila.Clave] = ReconstruirValor(fila.Valor);

            return configuracion;
        }

        private static string ConvertirValor(JsonElement elemento)
        {
            return elemento.ValueKind switch
            {
                JsonValueKind.String => elemento.GetString() ?? "",
                JsonValueKind.Number => elemento.TryGetInt32(out int i) ? i.ToString() : elemento.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => elemento.ToString()
            };
        }

        private static object ReconstruirValor(string valor)
        {
            if (bool.TryParse(valor, out var b)) return b;
            if (double.TryParse(valor, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            return valor;
        }

        private static string Resumir(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return "(vacío)";
            // Las imágenes/logo se almacenan en base64: no se registran completas en auditoría
            if (valor.StartsWith("data:image") || valor.Length > 60)
                return $"(texto largo, {valor.Length} caracteres)";
            return valor;
        }
    }
}
