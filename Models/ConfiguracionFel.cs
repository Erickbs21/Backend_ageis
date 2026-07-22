using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("configuracion_fel")]
    public class ConfiguracionFel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>NIT del emisor registrado ante la SAT</summary>
        [Required]
        [MaxLength(20)]
        [Column("nit_emisor")]
        public string NitEmisor { get; set; } = string.Empty;

        /// <summary>Usuario del API del certificador FEL</summary>
        [Required]
        [MaxLength(150)]
        [Column("usuario_api")]
        public string UsuarioApi { get; set; } = string.Empty;

        /// <summary>Llave o Token de autenticación del API FEL (almacenado cifrado)</summary>
        [Required]
        [Column("token_api")]
        public string TokenApi { get; set; } = string.Empty;

        /// <summary>Ambiente: "Pruebas" o "Produccion"</summary>
        [Required]
        [MaxLength(20)]
        [Column("ambiente")]
        public string Ambiente { get; set; } = "Pruebas";

        /// <summary>Nombre del certificador autorizado (p.ej. INFILE, G4S, Digifact)</summary>
        [Required]
        [MaxLength(100)]
        [Column("nombre_certificador")]
        public string NombreCertificador { get; set; } = string.Empty;

        /// <summary>Indica si la configuración FEL está activa</summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        /// <summary>Fecha de la última actualización</summary>
        [Column("fecha_actualizacion")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        /// <summary>Id del usuario que realizó la última modificación</summary>
        [Column("usuario_id")]
        public int? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
