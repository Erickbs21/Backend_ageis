using System.ComponentModel.DataAnnotations;

namespace ApiAegis.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string? Dpi { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public bool CreditoHabilitado { get; set; }
        public decimal LimiteCredito { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CrearClienteDto
    {
        [Required(ErrorMessage = "El NIT es requerido")]
        [MaxLength(50)]
        public string Nit { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Dpi { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Direccion { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Correo inválido")]
        [MaxLength(150)]
        public string? Correo { get; set; }

        public bool CreditoHabilitado { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor o igual a 0")]
        public decimal LimiteCredito { get; set; } = 0.00m;
    }

    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearProveedorDto
    {
        [Required(ErrorMessage = "El nombre del proveedor es requerido")]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El NIT es requerido")]
        [MaxLength(50)]
        public string Nit { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Correo inválido")]
        [MaxLength(150)]
        public string? Correo { get; set; }

        [MaxLength(255)]
        public string? Direccion { get; set; }
    }

    public class CrearCompraDto
    {
        [Required(ErrorMessage = "El proveedor es requerido")]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "Debe registrar al menos un detalle de compra")]
        public List<CrearCompraDetalleDto> Detalles { get; set; } = new List<CrearCompraDetalleDto>();
    }

    public class CrearCompraDetalleDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser de al menos 1")]
        public int Cantidad { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor a 0")]
        public decimal CostoUnitario { get; set; }
    }

    public class CompraDto
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCompra { get; set; }
        public List<CompraDetalleDto> Detalles { get; set; } = new List<CompraDetalleDto>();
    }

    public class CompraDetalleDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class MovimientoInventarioDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty; // ENTRADA, SALIDA, AJUSTE, DEVOLUCION
        public int Cantidad { get; set; }
        public int ExistenciaAnterior { get; set; }
        public int ExistenciaNueva { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string? Observacion { get; set; }
        public DateTime FechaMovimiento { get; set; }
    }

    public class RegistrarMovimientoManualDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        public string TipoMovimiento { get; set; } = string.Empty; // ENTRADA, SALIDA, AJUSTE

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser de al menos 1")]
        public int Cantidad { get; set; }

        [MaxLength(255)]
        public string? Observacion { get; set; }
    }

    public class CrearDevolucionDto
    {
        [Required(ErrorMessage = "La venta es requerida")]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El motivo es requerido")]
        [MaxLength(255)]
        public string Motivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe especificar los productos a devolver")]
        public List<CrearDevolucionDetalleDto> Detalles { get; set; } = new List<CrearDevolucionDetalleDto>();
    }

    public class CrearDevolucionDetalleDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad a devolver debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }

    public class DevolucionDto
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string VentaNumeroDocumento { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public List<DevolucionDetalleDto> Detalles { get; set; } = new List<DevolucionDetalleDto>();
    }

    public class DevolucionDetalleDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
