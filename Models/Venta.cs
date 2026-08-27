using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("ventas")]
    public class Venta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("numero_documento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required]
        [Column("metodo_pago_id")]
        public int MetodoPagoId { get; set; }

        [ForeignKey("MetodoPagoId")]
        public MetodoPago? MetodoPago { get; set; }

        [MaxLength(150)]
        [Column("nombre_cliente")]
        public string? NombreCliente { get; set; }

        [MaxLength(25)]
        [Column("nit")]
        public string? Nit { get; set; }

        [MaxLength(20)]
        [Column("tipo_documento")]
        public string TipoDocumento { get; set; } = "VENTA"; // VENTA, COTIZACION

        [Column("subtotal")]
        public decimal Subtotal { get; set; } = 0.00m;

        [Column("descuento")]
        public decimal Descuento { get; set; } = 0.00m;

        [Column("impuestos")]
        public decimal Impuestos { get; set; } = 0.00m;

        [Column("total")]
        public decimal Total { get; set; } = 0.00m;

        [Column("vuelto")]
        public decimal Vuelto { get; set; } = 0.00m;

        [Required]
        [MaxLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "PAGADA"; // PENDIENTE, PAGADA, ANULADA

        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

        // Relaciones
        public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
        
        public ICollection<VentaPago> Pagos { get; set; } = new List<VentaPago>();
    }
}
