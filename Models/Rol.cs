using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("roles")]
    public class Rol
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
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}
