using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("rol_permisos")]
    public class RolPermiso
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("rol_id")]
        public int RolId { get; set; }

        [ForeignKey("RolId")]
        public Rol? Rol { get; set; }

        [Required]
        [Column("permiso_id")]
        public int PermisoId { get; set; }

        [ForeignKey("PermisoId")]
        public Permiso? Permiso { get; set; }
    }
}
