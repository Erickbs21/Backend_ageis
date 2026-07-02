using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("devoluciones_detalle")]
    public class DevolucionDetalle
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("devolucion_id")]
        public int DevolucionId { get; set; }

        [ForeignKey("DevolucionId")]
        public Devolucion? Devolucion { get; set; }

        [Required]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }
    }
}
