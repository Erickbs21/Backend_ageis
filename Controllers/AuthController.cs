using System;
using Microsoft.AspNetCore.Mvc;
using ApiAegis.Models;
using ApiAegis.DAO;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly UserDao _userDao;

        public AuthController(UserDao userDao)
        {
            _userDao = userDao;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validación: Faltan Datos (400 Bad Request)
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    // "El botón de "Acceder" se deshabilita automáticamente si los campos están vacíos."
                    return BadRequest(new { Message = "Bad Request: Faltan datos requeridos." });
                }

                // Autenticar a través de Base de Datos
                UserModel user = _userDao.AutenticarUsuario(request.Username, request.Password);

                // Validación: Credenciales Inválidas (401 Unauthorized)
                if (user == null)
                {
                    return Unauthorized(new { Message = "ACCESO DENEGADO. CREDENCIALES INVÁLIDAS." });
                }

                // Generar token JWT simulado (según los requerimientos)
                string fakeToken = "fake-jwt-token-xyz-123456";

                LoginResponse response = new LoginResponse(user, fakeToken);

                // Respuesta Exitosa (200 OK)
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Validación: Servidor Caído / Network Error (500 Internal Server Error)
                // Se registra el error internamente (ideal logger)
                Console.WriteLine($"Error de servidor: {ex.Message}");
                // Se responde según lo esperado
                return StatusCode(500, new { Message = "ERROR DE CONEXIÓN CON EL SERVIDOR." });
            }
        }
        
        // Retrocompatibilidad con firma anterior que habías pedido revisar
        [HttpPost("autenticarMedios")]
        public IActionResult AutenticarMedios(string codUsuario, string clave, string UID, string dispositivo, string tipoDispositivo, string versionSO, string versionApp, string tipoLogin, string Comercio, string Agencia, string Usuario, string Password)
        {
            try
            {
                 // Puedes mapear los parámetros acá o usar la nueva firma en función de lo que realmente necesitas.
                 // Retornamos una respuesta dummy en base a tu ejemplo o invocar _userDao.AutenticarMedios(...)
                 return Ok(new { Respuesta = "Ejemplo de retrocompatibilidad activado" });
            }
            catch (Exception ex) 
            {
                 return StatusCode(500, new { Message = "ERROR DE CONEXIÓN CON EL SERVIDOR.", Details = ex.Message });
            }
        }
    }
}
