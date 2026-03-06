using System;
using Microsoft.AspNetCore.Mvc;
using ApiAegis.Models;
using ApiAegis.DAO;
using Microsoft.AspNetCore.Authorization;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Gerente")]
    public class ProvidersController : ControllerBase
    {
        #region Properties & Constructor
        private readonly ProviderDao _providerDao;

        public ProvidersController(ProviderDao providerDao)
        {
            _providerDao = providerDao;
        }
        #endregion

        #region Endpoints GET
        [HttpGet]
        public IActionResult GetProviders()
        {
            try
            {
                var providers = _providerDao.ObtenerProveedores();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener proveedores.", Details = ex.Message });
            }
        }
        #endregion

        #region Endpoints POST
        [HttpPost]
        public IActionResult CreateProvider([FromBody] CreateProviderRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.NitRfc))
                {
                    return BadRequest(new { Message = "Datos incompletos. Nombre y NIT/RFC son obligatorios." });
                }

                int newId = _providerDao.CrearProveedor(request);
                
                if (newId > 0)
                {
                    return CreatedAtAction(nameof(GetProviders), new { id = newId }, new { Id = newId, Message = "Proveedor creado exitosamente." });
                }
                
                return BadRequest(new { Message = "No se pudo crear el proveedor." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al crear proveedor.", Details = ex.Message });
            }
        }
        #endregion

        #region Endpoints PUT
        [HttpPut("{id}")]
        public IActionResult UpdateProvider(int id, [FromBody] CreateProviderRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.NitRfc))
                {
                    return BadRequest(new { Message = "Datos incompletos para actualizar." });
                }

                bool success = _providerDao.ActualizarProveedor(id, request);
                
                if (success)
                {
                    return Ok(new { Message = "Proveedor actualizado correctamente." });
                }
                
                return NotFound(new { Message = "Proveedor no encontrado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al actualizar proveedor.", Details = ex.Message });
            }
        }
        #endregion
    }
}
