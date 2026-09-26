using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("caja_movimientos")]
    public class CajaMovimiento
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("caja_apertura_id")]
        public int CajaAperturaId { get; set; }

        [ForeignKey("CajaAperturaId")]
        public CajaApertura? CajaApertura { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("tipo")]
        public string Tipo { get; set; } = "ENTRADA"; // ENTRADA, SALIDA

        [Required]
        [Column("monto")]
        public decimal Monto { get; set; } = 0.00m;

        [Required]
        [MaxLength(255)]
        [Column("motivo")]
        public string Motivo { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("observacion")]
        public string? Observacion { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
