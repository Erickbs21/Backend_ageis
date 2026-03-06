using System;
using Microsoft.AspNetCore.Mvc;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        #region Endpoints GET
        [HttpGet("time")]
        public IActionResult GetTime()
        {
            try
            {
                // Zona horaria de Guatemala UTC-6 (Central America Standard Time)
                TimeZoneInfo guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                DateTime guatemalaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                
                return Ok(new 
                { 
                    ServerTimeGuatemala = guatemalaTime,
                    Iso8601 = guatemalaTime.ToString("o"),
                    Timezone = "UTC-6 (Guatemala)"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener la hora del servidor.", Details = ex.Message });
            }
        }
        #endregion
    }
}
