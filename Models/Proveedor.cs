using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("proveedores")]
    public class Proveedor
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("nit")]
        public string Nit { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        [Column("correo")]
        public string? Correo { get; set; }

        [MaxLength(255)]
        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
