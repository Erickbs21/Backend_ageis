using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        [Column("correo")]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("rol_id")]
        public int RolId { get; set; }

        [ForeignKey("RolId")]
        public Rol? Rol { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("ultimo_acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        [Column("refresh_token")]
        public string? RefreshToken { get; set; }

        [Column("refresh_token_expira")]
        public DateTime? RefreshTokenExpira { get; set; }
    }
}
