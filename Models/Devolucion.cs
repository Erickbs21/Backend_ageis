using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("devoluciones")]
    public class Devolucion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("venta_id")]
        public int VentaId { get; set; }

        [ForeignKey("VentaId")]
        public Venta? Venta { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("motivo")]
        public string Motivo { get; set; } = string.Empty;

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Relaciones
        public ICollection<DevolucionDetalle> Detalles { get; set; } = new List<DevolucionDetalle>();
    }
}
