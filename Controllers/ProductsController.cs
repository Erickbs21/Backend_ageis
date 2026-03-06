using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ApiAegis.Models;
using ApiAegis.DAO;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;


namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        #region Properties & Constructor
        private readonly ProductDao _productDao;

        public ProductsController(ProductDao productDao)
        {
            _productDao = productDao;
        }
        #endregion

        #region Endpoints GET
        [HttpGet]
        public IActionResult GetProducts()
        {
            try
            {
                var products = _productDao.ObtenerProductos();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener productos.", Details = ex.Message });
            }
        }
        #endregion

        #region Endpoints POST
        [HttpPost]
        [Authorize(Roles = "Admin,Gerente")]
        public IActionResult CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.CodigoBarras) || string.IsNullOrWhiteSpace(request.Nombre))
                {
                    return BadRequest(new { Message = "Datos incompletos para crear el producto. Código de barras y Nombre son obligatorios." });
                }

                int newId = _productDao.CrearProducto(request);
                
                if (newId > 0)
                {
                    return CreatedAtAction(nameof(GetProducts), new { id = newId }, new { Id = newId, Message = "Producto creado exitosamente." });
                }
                
                return BadRequest(new { Message = "No se pudo crear el producto." });
            }
            catch (SqlException sqlEx) when (sqlEx.Message.Contains("código de barras ya está registrado"))
            {
                return BadRequest(new { Message = "El código de barras ya está registrado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al crear producto.", Details = ex.Message });
            }
        }
        #endregion

        #region Endpoints PUT
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Gerente")]
        public IActionResult UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { Message = "Datos incompletos para actualizar el producto." });
                }

                bool success = _productDao.ActualizarProducto(id, request);
                
                if (success)
                {
                    return Ok(new { Message = "Producto actualizado correctamente." });
                }
                
                return NotFound(new { Message = "Producto no encontrado o inactivo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al actualizar producto.", Details = ex.Message });
            }
        }
        #endregion

        #region Endpoints DELETE
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Gerente")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                bool success = _productDao.EliminarProducto(id);
                
                if (success)
                {
                    return Ok(new { Message = "Producto eliminado (marcado como inactivo) correctamente." });
                }
                
                return NotFound(new { Message = "Producto no encontrado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al eliminar producto.", Details = ex.Message });
            }
        }
        #endregion
    }
}
