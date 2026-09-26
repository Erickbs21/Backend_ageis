using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAegis.Models
{
    [Table("caja_cierres")]
    public class CajaCierre
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("caja_apertura_id")]
        public int? CajaAperturaId { get; set; }

        [ForeignKey("CajaAperturaId")]
        public CajaApertura? CajaApertura { get; set; }

        [Required]
        [Column("caja_id")]
        public int CajaId { get; set; }

        [ForeignKey("CajaId")]
        public Caja? Caja { get; set; }

        // Quién cerró la caja
        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required]
        [Column("monto_final")]
        public decimal MontoFinal { get; set; }

        // ====== Resumen de ventas de la sesión ======
        [Column("fondo_inicial")]
        public decimal FondoInicial { get; set; }

        [Column("total_ventas")]
        public decimal TotalVentas { get; set; }

        [Column("cantidad_ventas")]
        public int CantidadVentas { get; set; }

        [Column("ventas_efectivo")]
        public decimal VentasEfectivo { get; set; }

        [Column("ventas_tarjeta")]
        public decimal VentasTarjeta { get; set; }

        [Column("ventas_transferencia")]
        public decimal VentasTransferencia { get; set; }

        [Column("ventas_cheque")]
        public decimal VentasCheque { get; set; }

        [Column("ventas_credito")]
        public decimal VentasCredito { get; set; }

        [Column("ventas_mixto")]
        public decimal VentasMixto { get; set; }

        [Column("ventas_anuladas")]
        public decimal VentasAnuladas { get; set; }

        [Column("devoluciones")]
        public decimal Devoluciones { get; set; }

        // ====== Movimientos de caja ======
        [Column("entradas_efectivo")]
        public decimal EntradasEfectivo { get; set; }

        [Column("salidas_efectivo")]
        public decimal SalidasEfectivo { get; set; }

        // ====== Cuadre ======
        [Column("efectivo_esperado")]
        public decimal EfectivoEsperado { get; set; }

        [Column("efectivo_contado")]
        public decimal EfectivoContado { get; set; }

        [Column("diferencia")]
        public decimal Diferencia { get; set; }

        // CUADRADO, FALTANTE, SOBRANTE
        [Required]
        [MaxLength(20)]
        [Column("estado_corte")]
        public string EstadoCorte { get; set; } = "CUADRADO";

        // ====== Conteo físico de efectivo ======
        [Column("billetes_q200")]
        public int BilletesQ200 { get; set; }

        [Column("billetes_q100")]
        public int BilletesQ100 { get; set; }

        [Column("billetes_q50")]
        public int BilletesQ50 { get; set; }

        [Column("billetes_q20")]
        public int BilletesQ20 { get; set; }

        [Column("billetes_q10")]
        public int BilletesQ10 { get; set; }

        [Column("billetes_q5")]
        public int BilletesQ5 { get; set; }

        [Column("monedas")]
        public decimal Monedas { get; set; }

        [Column("total_conteo")]
        public decimal TotalConteo { get; set; }

        // ====== Notas ======
        [MaxLength(500)]
        [Column("notas")]
        public string? Notas { get; set; }

        [Column("fecha_cierre")]
        public DateTime FechaCierre { get; set; } = DateTime.UtcNow;

        [MaxLength(10)]
        [Column("tipo_corte")]
        public string TipoCorte { get; set; } = "NORMAL"; // NORMAL, PARCIAL
    }
}
