using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiAegis.Helpers;
using System.Text.Json;

namespace ApiAegis.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracionController : ControllerBase
    {
        private static readonly Dictionary<string, object> _configuracionGeneral = new()
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

        [HttpGet]
        public IActionResult GetConfiguracion()
        {
            return Ok(_configuracionGeneral);
        }

        [HttpPost]
        [HttpPut]
        [TienePermiso("GestionConfiguracion")]
        public IActionResult GuardarConfiguracion([FromBody] Dictionary<string, JsonElement> nuevaConfiguracion)
        {
            if (nuevaConfiguracion == null)
                return BadRequest(new { mensaje = "Configuración inválida" });

            foreach (var kvp in nuevaConfiguracion)
            {
                object value = kvp.Value.ValueKind switch
                {
                    JsonValueKind.String => kvp.Value.GetString() ?? "",
                    JsonValueKind.Number => kvp.Value.TryGetInt32(out int i) ? i : kvp.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => kvp.Value.ToString()
                };
                _configuracionGeneral[kvp.Key] = value;
            }

            return Ok(new { mensaje = "Configuración guardada correctamente", datos = _configuracionGeneral });
        }
    }
}
