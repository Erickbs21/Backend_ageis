using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ApiAegis.Models;
using ApiAegis.DAO;
using Microsoft.AspNetCore.Authorization;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // El control se hará por Endpoint en lugar de global en la clase
    public class UsersController : ControllerBase
    {
        #region Properties & Constructor
        private readonly UserDao _userDao;

        public UsersController(UserDao userDao)
        {
            _userDao = userDao;
        }
        #endregion

        #region Endpoints GET
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            try
            {
                List<UserModel> users = _userDao.ObtenerUsuarios();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener usuarios.", Details = ex.Message });
            }
        }
        #endregion

        #region Endpoints POST
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { Message = "Datos inválidos para crear usuario." });
                }

                if (request.Role != "Admin" && request.Role != "Gerente" && request.Role != "Empleado")
                {
                    return BadRequest(new { Message = "Rol inválido. Debe ser: Admin, Gerente o Empleado." });
                }

                var newUser = new UserModel
                {
                    Name = request.Nombre,
                    Username = request.Username,
                    Role = request.Role
                };

                int newId = _userDao.CrearUsuario(newUser, request.Password);
                
                if (newId > 0)
                {
                    newUser.Id = newId;
                    return CreatedAtAction(nameof(GetUsers), new { id = newId }, newUser);
                }
                
                return BadRequest(new { Message = "No se pudo crear el usuario. Posible nombre de usuario duplicado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al crear el usuario.", Details = ex.Message });
            }
        }

        [HttpPost("{id}/reset")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetPassword(int id, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { Message = "La nueva contraseña es requerida." });
                }

                bool success = _userDao.ResetearPassword(id, request.NewPassword);
                
                if (success)
                {
                    return Ok(new { Message = "Contraseña reiniciada correctamente." });
                }
                
                return NotFound(new { Message = "Usuario no encontrado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al reiniciar la contraseña.", Details = ex.Message });
            }
        }
        [HttpGet("me")]
        public IActionResult GetMyProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var usernameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { Message = "Token inválido o expirado." });
                }

                return Ok(new 
                { 
                    Id = int.Parse(userIdClaim.Value),
                    Username = usernameClaim?.Value,
                    Role = roleClaim?.Value,
                    Message = "Perfil recuperado desde JWT."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener perfil del usuario.", Details = ex.Message });
            }
        }
        #endregion
    }
}
