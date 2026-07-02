using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("facturas")]
    public class Factura
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
        [MaxLength(50)]
        [Column("serie")]
        public string Serie { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("numero")]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("uuid")]
        public string Uuid { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("estado")]
        public string Estado { get; set; } = "EMITIDA"; // EMITIDA, ANULADA

        [Column("fecha_emision")]
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    }
}
