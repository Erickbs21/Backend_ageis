using System.ComponentModel.DataAnnotations;

namespace ApiAegis.DTOs
{
    public class CarteraCreditoDto
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal Disponible { get; set; }
        public int VentasCredito { get; set; }
        public DateTime? UltimaVenta { get; set; }
    }

    public class CreditoVentaDto
    {
        public int VentaId { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public decimal Abonado { get; set; }
        public decimal Saldo { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class AbonoCreditoDto
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? ClienteNombre { get; set; }
        public int MetodoPagoId { get; set; }
        public string MetodoPagoNombre { get; set; } = string.Empty;
        public string? UsuarioNombre { get; set; }
        public decimal Monto { get; set; }
        public string? Observacion { get; set; }
        public DateTime FechaAbono { get; set; }
    }

    public class RegistrarAbonoDto
    {
        [Required(ErrorMessage = "La venta es requerida")]
        public int VentaId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto del abono debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El método de pago es requerido")]
        public int MetodoPagoId { get; set; }

        [MaxLength(255, ErrorMessage = "La observación no puede exceder 255 caracteres")]
        public string? Observacion { get; set; }
    }
}
