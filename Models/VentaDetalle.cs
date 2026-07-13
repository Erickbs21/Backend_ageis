using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("ventas_detalle")]
    public class VentaDetalle
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
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("descuento")]
        public decimal Descuento { get; set; } = 0.00m;

        [Required]
        [Column("subtotal")]
        public decimal Subtotal { get; set; }
    }
}
