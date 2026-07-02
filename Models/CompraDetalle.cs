using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("compras_detalle")]
    public class CompraDetalle
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("compra_id")]
        public int CompraId { get; set; }

        [ForeignKey("CompraId")]
        public Compra? Compra { get; set; }

        [Required]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("costo_unitario")]
        public decimal CostoUnitario { get; set; }

        [Required]
        [Column("subtotal")]
        public decimal Subtotal { get; set; }
    }
}
