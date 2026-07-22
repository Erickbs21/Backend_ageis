using System.ComponentModel.DataAnnotations;

namespace ApiAegis.DTOs
{
    /// <summary>DTO de respuesta para la configuración FEL (oculta el token)</summary>
    public class ConfiguracionFelDto
    {
        public int Id { get; set; }
        public string NitEmisor { get; set; } = string.Empty;
        public string UsuarioApi { get; set; } = string.Empty;

        /// <summary>Token enmascarado para no exponer la llave real</summary>
        public string TokenApiMascarado { get; set; } = string.Empty;

        public string Ambiente { get; set; } = string.Empty;
        public string NombreCertificador { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public string? UsuarioModifico { get; set; }
    }

    /// <summary>DTO para crear o actualizar la configuración FEL</summary>
    public class GuardarConfiguracionFelDto
    {
        [Required(ErrorMessage = "El NIT del emisor es requerido")]
        [MaxLength(20, ErrorMessage = "El NIT no puede exceder 20 caracteres")]
        public string NitEmisor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El usuario del API es requerido")]
        [MaxLength(150, ErrorMessage = "El usuario del API no puede exceder 150 caracteres")]
        public string UsuarioApi { get; set; } = string.Empty;

        /// <summary>
        /// Token o llave del API FEL. Si se envía vacío al actualizar, se conserva el token existente.
        /// </summary>
        [MaxLength(500)]
        public string? TokenApi { get; set; }

        [Required(ErrorMessage = "El ambiente es requerido")]
        [RegularExpression("^(Pruebas|Produccion)$", ErrorMessage = "El ambiente debe ser 'Pruebas' o 'Produccion'")]
        public string Ambiente { get; set; } = "Pruebas";

        [Required(ErrorMessage = "El nombre del certificador es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre del certificador no puede exceder 100 caracteres")]
        public string NombreCertificador { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }

    /// <summary>DTO de respuesta para la prueba de conexión al API FEL</summary>
    public class PruebaConexionFelResponseDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Ambiente { get; set; } = string.Empty;
        public string Certificador { get; set; } = string.Empty;
        public DateTime FechaPrueba { get; set; } = DateTime.UtcNow;
    }
}
