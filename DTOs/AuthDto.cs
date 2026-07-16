using System.ComponentModel.DataAnnotations;

namespace ApiAegis.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public List<string> Permisos { get; set; } = new List<string>();
    }

    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "El token de acceso es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "El refresh token es requerido")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class CambiarPasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NuevoPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NuevoPassword { get; set; } = string.Empty;
    }
}
