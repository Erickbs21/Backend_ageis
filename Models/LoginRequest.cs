using System;
using System.ComponentModel.DataAnnotations;

namespace ApiAegis.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El campo Username es obligatorio")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El campo Password es obligatorio")]
        public string Password { get; set; }

        // Puedes agregar más propiedades para recoger metadata del dispositivo si se necesita después.

        public LoginRequest()
        {
        }

        public LoginRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
