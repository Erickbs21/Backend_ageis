using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("auditoria")]
    public class Auditoria
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("accion")]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("tabla_afectada")]
        public string TablaAfectada { get; set; } = string.Empty;

        [Column("registro_id")]
        public int? RegistroId { get; set; }

        [MaxLength(45)]
        [Column("ip")]
        public string? Ip { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
