using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("compras")]
    public class Compra
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("proveedor_id")]
        public int ProveedorId { get; set; }

        [ForeignKey("ProveedorId")]
        public Proveedor? Proveedor { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Column("subtotal")]
        public decimal Subtotal { get; set; } = 0.00m;

        [Column("impuestos")]
        public decimal Impuestos { get; set; } = 0.00m;

        [Column("total")]
        public decimal Total { get; set; } = 0.00m;

        [Column("fecha_compra")]
        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

        // Relaciones
        public ICollection<CompraDetalle> Detalles { get; set; } = new List<CompraDetalle>();
    }
}
