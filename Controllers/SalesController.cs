using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using ApiAegis.Models;
using ApiAegis.DAO;
using Microsoft.AspNetCore.Authorization;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        #region Properties & Constructor
        private readonly SaleDao _saleDao;

        public SalesController(SaleDao saleDao)
        {
            _saleDao = saleDao;
        }
        #endregion

        #region Endpoints POST
        [HttpPost]
        public IActionResult RegisterSale([FromBody] CreateSaleRequest request)
        {
            try
            {
                if (request == null || request.Detalles == null || request.Detalles.Count == 0)
                {
                    return BadRequest(new { Message = "La venta debe contener al menos un producto." });
                }

                // Aquí en el futuro, request.IdUsuario se tomará del token JWT User.Claims
                if (request.IdUsuario <= 0)
                {
                    return BadRequest(new { Message = "Id de usuario (cajero) no válido." });
                }

                int newSaleId = _saleDao.CrearVentaCompleta(request);

                return CreatedAtAction(nameof(GetTicket), new { id = newSaleId }, new { Id = newSaleId, Message = "Venta registrada exitosamente." });
            }
            catch (SqlException sqlEx) when (sqlEx.Message.Contains("Stock insuficiente"))
            {
                // Se disparó el RAISEERROR en el Stored Procedure
                return BadRequest(new { Message = "Stock insuficiente para uno o más productos vendidos.", Details = sqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al registrar la venta.", Details = ex.Message });
            }
        }
        #endregion

        #region Endpoints GET
        [HttpGet("history")]
        [Authorize(Roles = "Admin,Gerente")]
        public IActionResult GetHistory([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] int? idUsuario)
        {
            try
            {
                var history = _saleDao.ObtenerHistorialVentas(fechaInicio, fechaFin, idUsuario);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener historial de ventas.", Details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetTicket(int id)
        {
            try
            {
                var ticketDetails = _saleDao.ObtenerTicketVenta(id);
                
                if (ticketDetails == null || ticketDetails.Count == 0)
                {
                    return NotFound(new { Message = "Ticket de venta no encontrado." });
                }

                return Ok(ticketDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener ticket de la venta.", Details = ex.Message });
            }
        }
        #endregion
    }
}
