using System;

namespace ApiAegis.Models
{
    public class DashboardStatsModel
    {
        public decimal VentasHoy { get; set; }
        public int AlertasStock { get; set; }
        public int ProveedoresActivos { get; set; }
    }
}
