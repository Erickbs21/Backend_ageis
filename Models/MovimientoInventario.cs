using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("movimientos_inventario")]
    public class MovimientoInventario
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("tipo_movimiento")]
        public string TipoMovimiento { get; set; } = string.Empty; // ENTRADA, SALIDA, AJUSTE, DEVOLUCION

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("existencia_anterior")]
        public int ExistenciaAnterior { get; set; }

        [Required]
        [Column("existencia_nueva")]
        public int ExistenciaNueva { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [MaxLength(255)]
        [Column("observacion")]
        public string? Observacion { get; set; }

        [Column("fecha_movimiento")]
        public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    }
}
