using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("metodos_pago")]
    public class MetodoPago
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty; // Efectivo, Tarjeta, Transferencia, Cheque, Crédito, Mixto

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
