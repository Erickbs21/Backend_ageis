using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ApiAegis.Models;
using ApiAegis.DAO;

namespace ApiAegis.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        #region Properties & Constructor
        private readonly UserDao _userDao;
        private readonly IConfiguration _config;

        public AuthController(UserDao userDao, IConfiguration config)
        {
            _userDao = userDao;
            _config = config;
        }
        #endregion

        #region Login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { Message = "Bad Request: Faltan datos requeridos." });
                }

                UserModel user = _userDao.AutenticarUsuario(request.Username, request.Password);

                if (user == null)
                {
                    return Unauthorized(new { Message = "ACCESO DENEGADO. CREDENCIALES INVÁLIDAS." });
                }

                // Generar token real JWT
                var keyBytes = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
                var claims = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                });
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    Expires = DateTime.UtcNow.AddHours(4),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _config["Jwt:Issuer"],
                    Audience = _config["Jwt:Audience"]
                };
                
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                string jwtToken = tokenHandler.WriteToken(token);

                LoginResponse response = new LoginResponse(user, jwtToken);

                return Ok(response);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error de servidor: {ex.Message}");
                return StatusCode(500, new { Message = "ERROR DE CONEXIÓN CON EL SERVIDOR." });
            }
        }
        #endregion

        #region Logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Invalida el token en el servidor
            // Dado que JWT es stateless, usualmente esto implica un token blocklist (guardar tokens revocados en BD o Cache)
            // Opcionalmente, se maneja sólo en FrontEnd si no hay un JWT Blocklist estricto.
            return Ok(new { Message = "Sesión cerrada correctamente. Token invalidado." });
        }
        #endregion
        
        #region Legacy Auth (Retrocompatibilidad)
        [HttpPost("autenticarMedios")]
        public IActionResult AutenticarMedios(string codUsuario, string clave, string UID, string dispositivo, string tipoDispositivo, string versionSO, string versionApp, string tipoLogin, string Comercio, string Agencia, string Usuario, string Password)
        {
            try
            {
                 return Ok(new { Respuesta = "Ejemplo de retrocompatibilidad activado" });
            }
            catch (Exception ex) 
            {
                 return StatusCode(500, new { Message = "ERROR DE CONEXIÓN CON EL SERVIDOR.", Details = ex.Message });
            }
        }
        #endregion
    }
}
