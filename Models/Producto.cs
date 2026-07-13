using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("codigo_barras")]
        public string? CodigoBarras { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("categoria_id")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        [MaxLength(100)]
        [Column("marca")]
        public string? Marca { get; set; }

        [Column("costo")]
        public decimal Costo { get; set; } = 0.00m;

        [Column("precio_venta")]
        public decimal PrecioVenta { get; set; } = 0.00m;

        [Column("precio_mayoreo")]
        public decimal PrecioMayoreo { get; set; } = 0.00m;

        [Column("stock_minimo")]
        public int StockMinimo { get; set; } = 0;

        [Column("stock_actual")]
        public int StockActual { get; set; } = 0;

        [Column("usa_codigo_barras")]
        public bool UsaCodigoBarras { get; set; } = false;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
