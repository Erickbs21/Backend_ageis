using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiAegis.Helpers;

namespace ApiAegis.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracionController : ControllerBase
    {
        private static object _configuracionGeneral = new
        {
            NombreEmpresa = "AEGIS POS",
            Nit = "12345678-9",
            Telefono = "7777-8888",
            Direccion = "Ciudad de Guatemala, Guatemala",
            Moneda = "GTQ",
            SimboloMoneda = "Q",
            ImpuestoPorcentaje = 12.0
        };

        [HttpGet]
        public IActionResult GetConfiguracion()
        {
            return Ok(_configuracionGeneral);
        }

        [HttpPost]
        [HttpPut]
        [TienePermiso("GestionConfiguracion")]
        public IActionResult GuardarConfiguracion([FromBody] object nuevaConfiguracion)
        {
            if (nuevaConfiguracion == null)
                return BadRequest(new { mensaje = "Configuración inválida" });

            _configuracionGeneral = nuevaConfiguracion;
            return Ok(new { mensaje = "Configuración guardada correctamente", datos = _configuracionGeneral });
        }
    }
}
