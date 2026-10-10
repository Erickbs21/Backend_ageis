using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("creditos_abonos")]
    public class AbonoCredito
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
        [Column("metodo_pago_id")]
        public int MetodoPagoId { get; set; }

        [ForeignKey("MetodoPagoId")]
        public MetodoPago? MetodoPago { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Column("caja_apertura_id")]
        public int? CajaAperturaId { get; set; }

        [ForeignKey("CajaAperturaId")]
        public CajaApertura? CajaApertura { get; set; }

        [Required]
        [Column("monto")]
        public decimal Monto { get; set; } = 0.00m;

        [MaxLength(255)]
        [Column("observacion")]
        public string? Observacion { get; set; }

        [Column("fecha_abono")]
        public DateTime FechaAbono { get; set; } = DateTime.UtcNow;
    }
}
