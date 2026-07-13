using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("permisos")]
    public class Permiso
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        // Relaciones
        public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}
