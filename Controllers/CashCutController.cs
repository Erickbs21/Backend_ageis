using System;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using ApiAegis.Models;
using ApiAegis.DAO;
using Microsoft.AspNetCore.Authorization;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/cash-cut")]
    [Authorize]
    public class CashCutController : ControllerBase
    {
        #region Properties & Constructor
        private readonly CashCutDao _cashCutDao;

        public CashCutController(CashCutDao cashCutDao)
        {
            _cashCutDao = cashCutDao;
        }
        #endregion

        #region Endpoints GET
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            try
            {
                var caja = _cashCutDao.ObtenerEstadoCaja();
                
                if (caja == null || caja.Estado != "Abierta")
                {
                    return Ok(new { Abierta = false, Message = "No hay caja abierta actualmente." });
                }

                return Ok(new 
                { 
                    Abierta = true, 
                    Id = caja.Id, 
                    SaldoInicial = caja.SaldoInicial, 
                    TotalVentas = caja.TotalVentas,
                    FechaApertura = caja.FechaApertura
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener estado de caja.", Details = ex.Message });
            }
        }
        #endregion

        #region Endpoints POST
        [HttpPost("open")]
        public IActionResult OpenBox([FromBody] OpenCashCutRequest request)
        {
            try
            {
                if (request == null || request.IdUsuario <= 0)
                {
                    return BadRequest(new { Message = "ID de usuario inválido." });
                }

                if (request.SaldoInicial < 0)
                {
                    return BadRequest(new { Message = "El saldo inicial no puede ser negativo." });
                }

                _cashCutDao.AbrirCaja(request.IdUsuario, request.SaldoInicial);
                
                return Ok(new { Message = "Caja abierta correctamente." });
            }
            catch (SqlException sqlEx) when (sqlEx.Message.Contains("Ya existe una caja abierta"))
            {
                return BadRequest(new { Message = "Ya existe una caja abierta, ciérrela primero." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al abrir la caja.", Details = ex.Message });
            }
        }

        [HttpPost("close")]
        public IActionResult CloseBox()
        {
            try
            {
                var caja = _cashCutDao.ObtenerEstadoCaja();
                
                if (caja == null || caja.Estado != "Abierta")
                {
                    return BadRequest(new { Message = "No hay caja abierta para cerrar." });
                }

                decimal saldoFinal = _cashCutDao.CerrarCaja(caja.Id);
                
                return Ok(new 
                { 
                    Message = "Caja cerrada correctamente.",
                    Resumen = new 
                    {
                        SaldoInicial = caja.SaldoInicial,
                        VentasTotales = caja.TotalVentas,
                        Egresos = caja.TotalEgresos,
                        SaldoFinal = saldoFinal
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al cerrar la caja.", Details = ex.Message });
            }
        }
        #endregion
    }
}
