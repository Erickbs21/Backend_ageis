using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("venta_pagos")]
    public class VentaPago
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
        [Column("monto")]
        public decimal Monto { get; set; } = 0.00m;

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    }
}
