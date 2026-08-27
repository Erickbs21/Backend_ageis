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
    }

    public class CierreCajaDto
    {
        [Required(ErrorMessage = "La caja es requerida")]
        public int CajaId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto final debe ser mayor o igual a 0")]
        public decimal MontoFinal { get; set; }

        public string? Notas { get; set; }
    }

    public class CrearCajaDto
    {
        [Required(ErrorMessage = "El nombre de la caja es requerido")]
        public string Nombre { get; set; } = string.Empty;

        public int? UsuarioId { get; set; }
    }

    public class CajaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
    }

    public class CajaDetalleSesionDto
    {
        public int CajaId { get; set; }
        public string CajaNombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
        public decimal MontoInicial { get; set; }
        public DateTime? FechaApertura { get; set; }
        public decimal VentasRegistradas { get; set; }
        public decimal MontoEsperado { get; set; }
    }
}
