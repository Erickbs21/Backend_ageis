using System.ComponentModel.DataAnnotations;

namespace ApiAegis.DTOs
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearCategoriaDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }
    }

    public class ProductoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? CodigoBarras { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public decimal Costo { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal PrecioMayoreo { get; set; }
        public int StockMinimo { get; set; }
        public int StockActual { get; set; }
        public bool UsaCodigoBarras { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CrearProductoDto
    {
        [Required(ErrorMessage = "El código es requerido")]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CodigoBarras { get; set; }

        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [MaxLength(100)]
        public string? Marca { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser mayor o igual a 0")]
        public decimal Costo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor o igual a 0")]
        public decimal PrecioVenta { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de mayoreo debe ser mayor o igual a 0")]
        public decimal PrecioMayoreo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor o igual a 0")]
        public int StockMinimo { get; set; }

        public bool UsaCodigoBarras { get; set; } = false;
    }
}
