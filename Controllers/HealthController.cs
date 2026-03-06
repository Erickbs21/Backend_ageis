using Microsoft.AspNetCore.Mvc;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Status = "Alive", Message = "La API está funcionando correctamente." });
        }
    }
}
