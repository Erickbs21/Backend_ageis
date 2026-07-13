using System.ComponentModel.DataAnnotations;

namespace ApiAegis.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CrearUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [MaxLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        public int RolId { get; set; }
    }

    public class EditarUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [MaxLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        public int RolId { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public List<PermisoDto> Permisos { get; set; } = new List<PermisoDto>();
    }

    public class PermisoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class AsignarPermisosDto
    {
        [Required]
        public List<int> PermisosIds { get; set; } = new List<int>();
    }
}
