using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiAegis.DAO;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Cualquier usuario autenticado puede ver el dashboard (Opcional: cambiar a Solo Admin/Gerente)
    public class DashboardController : ControllerBase
    {
        #region Properties & Constructor
        private readonly DashboardDao _dashboardDao;

        public DashboardController(DashboardDao dashboardDao)
        {
            _dashboardDao = dashboardDao;
        }
        #endregion

        #region Endpoints GET
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            try
            {
                var stats = _dashboardDao.ObtenerEstadisticas();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener estadísticas del dashboard.", Details = ex.Message });
            }
        }
        #endregion
    }
}
