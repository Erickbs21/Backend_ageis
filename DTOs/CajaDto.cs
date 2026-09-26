using System.ComponentModel.DataAnnotations;

namespace ApiAegis.DTOs
{
    public class AperturaCajaDto
    {
        [Required(ErrorMessage = "La caja es requerida")]
        public int CajaId { get; set; }

        public int? UsuarioId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto inicial debe ser mayor o igual a 0")]
        public decimal MontoInicial { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }

    public class CierreCajaDto
    {
        [Required(ErrorMessage = "La caja es requerida")]
        public int CajaId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto final debe ser mayor o igual a 0")]
        public decimal MontoFinal { get; set; }

        public string? Notas { get; set; }

        // Conteo físico de efectivo
        public int BilletesQ200 { get; set; }
        public int BilletesQ100 { get; set; }
        public int BilletesQ50 { get; set; }
        public int BilletesQ20 { get; set; }
        public int BilletesQ10 { get; set; }
        public int BilletesQ5 { get; set; }
        public decimal Monedas { get; set; }
        public decimal TotalConteo { get; set; }

        // Resumen
        public decimal FondoInicial { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal VentasEfectivo { get; set; }
        public decimal VentasTarjeta { get; set; }
        public decimal VentasTransferencia { get; set; }
        public decimal VentasCheque { get; set; }
        public decimal VentasCredito { get; set; }
        public decimal VentasMixto { get; set; }
        public decimal VentasAnuladas { get; set; }
        public decimal Devoluciones { get; set; }
        public decimal EntradasEfectivo { get; set; }
        public decimal SalidasEfectivo { get; set; }
        public decimal EfectivoEsperado { get; set; }
        public decimal EfectivoContado { get; set; }
        public decimal Diferencia { get; set; }
        public string EstadoCorte { get; set; } = "CUADRADO";
        public string TipoCorte { get; set; } = "NORMAL";
    }

    public class CrearCajaDto
    {
        [Required(ErrorMessage = "El nombre de la caja es requerido")]
        public string Nombre { get; set; } = string.Empty;

        public int? UsuarioId { get; set; }
    }

    public class AsignarVendedorDto
    {
        [Required(ErrorMessage = "La caja es requerida")]
        public int CajaId { get; set; }

        [Required(ErrorMessage = "El vendedor es requerido")]
        public int UsuarioId { get; set; }
    }

    public class CajaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
    }

    public class CrearMovimientoDto
    {
        [Required(ErrorMessage = "La apertura de caja es requerida")]
        public int CajaAperturaId { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        [RegularExpression("^(ENTRADA|SALIDA)$", ErrorMessage = "El tipo debe ser ENTRADA o SALIDA")]
        public string Tipo { get; set; } = "ENTRADA";

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio")]
        [MaxLength(255)]
        public string Motivo { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Observacion { get; set; }
    }

    public class CajaMovimientoDto
    {
        public int Id { get; set; }
        public int CajaAperturaId { get; set; }
        public int UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? Observacion { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class CajaDetalleSesionDto
    {
        public int CajaId { get; set; }
        public int? CajaAperturaId { get; set; }
        public string CajaNombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
        public decimal MontoInicial { get; set; }
        public DateTime? FechaApertura { get; set; }
        public string? Observaciones { get; set; }
        public int? AsignadoPor { get; set; }
        public string? AsignadoPorNombre { get; set; }

        // Resumen de ventas de la sesión
        public decimal VentasRegistradas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal VentasEfectivo { get; set; }
        public decimal VentasTarjeta { get; set; }
        public decimal VentasTransferencia { get; set; }
        public decimal VentasCheque { get; set; }
        public decimal VentasCredito { get; set; }
        public decimal VentasMixto { get; set; }
        public decimal VentasAnuladas { get; set; }
        public decimal Devoluciones { get; set; }

        // Movimientos
        public decimal EntradasEfectivo { get; set; }
        public decimal SalidasEfectivo { get; set; }

        // Cuadre
        public decimal EfectivoEsperado { get; set; }
        public decimal MontoEsperado { get; set; }
        public int MovimientosCount { get; set; }
    }

    public class CierreResumenDto
    {
        public int CajaId { get; set; }
        public int? CajaAperturaId { get; set; }
        public string CajaNombre { get; set; } = string.Empty;
        public string? VendedorNombre { get; set; }
        public DateTime? FechaApertura { get; set; }
        public decimal FondoInicial { get; set; }

        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal VentasEfectivo { get; set; }
        public decimal VentasTarjeta { get; set; }
        public decimal VentasTransferencia { get; set; }
        public decimal VentasCheque { get; set; }
        public decimal VentasCredito { get; set; }
        public decimal VentasMixto { get; set; }
        public decimal VentasAnuladas { get; set; }
        public decimal Devoluciones { get; set; }

        public decimal EntradasEfectivo { get; set; }
        public decimal SalidasEfectivo { get; set; }

        public decimal EfectivoEsperado { get; set; }
        public int MovimientosCount { get; set; }
    }

    public class CorteCajaDto
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public string CajaNombre { get; set; } = string.Empty;
        public int? CajaAperturaId { get; set; }
        public DateTime? FechaApertura { get; set; }
        public DateTime FechaCierre { get; set; }
        public string? AbiertoPor { get; set; }
        public string? CerradoPor { get; set; }
        public decimal FondoInicial { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal VentasEfectivo { get; set; }
        public decimal VentasTarjeta { get; set; }
        public decimal VentasTransferencia { get; set; }
        public decimal VentasCheque { get; set; }
        public decimal VentasCredito { get; set; }
        public decimal VentasMixto { get; set; }
        public decimal VentasAnuladas { get; set; }
        public decimal Devoluciones { get; set; }
        public decimal EntradasEfectivo { get; set; }
        public decimal SalidasEfectivo { get; set; }
        public decimal EfectivoEsperado { get; set; }
        public decimal EfectivoContado { get; set; }
        public decimal Diferencia { get; set; }
        public string EstadoCorte { get; set; } = "CUADRADO";
        public string? Notas { get; set; }
        public int BilletesQ200 { get; set; }
        public int BilletesQ100 { get; set; }
        public int BilletesQ50 { get; set; }
        public int BilletesQ20 { get; set; }
        public int BilletesQ10 { get; set; }
        public int BilletesQ5 { get; set; }
        public decimal Monedas { get; set; }
        public decimal TotalConteo { get; set; }
        public string TipoCorte { get; set; } = "NORMAL";
        public int MovimientosCount { get; set; }
    }
}
