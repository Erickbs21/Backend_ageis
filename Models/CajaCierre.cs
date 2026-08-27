using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("caja_cierres")]
    public class CajaCierre
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("caja_id")]
        public int CajaId { get; set; }

        [ForeignKey("CajaId")]
        public Caja? Caja { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required]
        [Column("monto_final")]
        public decimal MontoFinal { get; set; }

        [Column("notas")]
        public string? Notas { get; set; }

        [Column("fecha_cierre")]
        public DateTime FechaCierre { get; set; } = DateTime.UtcNow;
    }
}
