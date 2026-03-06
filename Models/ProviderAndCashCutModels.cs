using System;

namespace ApiAegis.Models
{
    #region ProviderModels
    public class ProviderModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string NitRfc { get; set; }
        public string Telefono { get; set; }
    }

    public class CreateProviderRequest
    {
        public string Nombre { get; set; }
        public string NitRfc { get; set; }
        public string Telefono { get; set; }
    }
    #endregion

    #region CashCutModels
    public class CashCutModel
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal? SaldoFinal { get; set; }
        public string Estado { get; set; }
    }

    public class OpenCashCutRequest
    {
        public int IdUsuario { get; set; }
        public decimal SaldoInicial { get; set; }
    }
    #endregion
}
