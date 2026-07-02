using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("nit")]
        public string Nit { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("dpi")]
        public string? Dpi { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("direccion")]
        public string? Direccion { get; set; }

        [MaxLength(50)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        [Column("correo")]
        public string? Correo { get; set; }

        [Column("credito_habilitado")]
        public bool CreditoHabilitado { get; set; } = false;

        [Column("limite_credito")]
        public decimal LimiteCredito { get; set; } = 0.00m;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
