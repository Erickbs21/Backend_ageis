using System.ComponentModel.DataAnnotations;

namespace ApiAegis.DTOs
{
    public class CrearVentaDto
    {
        [Required(ErrorMessage = "El cliente es requerido")]
        public int ClienteId { get; set; } = 1; // 1 por defecto (Consumidor Final)

        public string? NombreCliente { get; set; }

        public string? Nit { get; set; }

        public string TipoDocumento { get; set; } = "VENTA"; // VENTA, COTIZACION, SUSPENDIDA

        public decimal Descuento { get; set; } = 0.00m;

        [Required(ErrorMessage = "La venta debe contener al menos un producto")]
        public List<CrearVentaDetalleDto> Detalles { get; set; } = new List<CrearVentaDetalleDto>();

        // Optional list of payments for mixed payments
        public List<CrearVentaPagoDto> Pagos { get; set; } = new List<CrearVentaPagoDto>();
    }

    public class CrearVentaPagoDto
    {
        public int MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
    }

    public class CrearVentaDetalleDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser de al menos 1")]
        public int Cantidad { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        public decimal Descuento { get; set; } = 0.00m;
    }

    public class VentaDto
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string? NombreCliente { get; set; }
        public string? Nit { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public decimal Vuelto { get; set; }
        public decimal Faltante { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public List<VentaDetalleDto> Detalles { get; set; } = new List<VentaDetalleDto>();
        public List<VentaPagoDto> Pagos { get; set; } = new List<VentaPagoDto>();
    }

    public class VentaPagoDto
    {
        public int Id { get; set; }
        public int MetodoPagoId { get; set; }
        public string MetodoPagoNombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
    }

    public class VentaDetalleDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string ProductoCodigo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class FacturaDto
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Serie { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Uuid { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
    }
}
