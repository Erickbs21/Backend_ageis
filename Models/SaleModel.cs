using System;
using System.Collections.Generic;

namespace ApiAegis.Models
{
    #region Sale Models
    public class SaleModel
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Cajero { get; set; } // Nombre del usuario
        public int? IdCorteCaja { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaVenta { get; set; }
        public string Estado { get; set; }
    }

    public class SaleDetailModel
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public string Producto { get; set; } // Nombre del producto
        public string CodigoBarras { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
    #endregion

    #region Requests DTOs
    public class CreateSaleRequest
    {
        public int IdUsuario { get; set; } // En un entorno real, esto viene del Token JWT
        public List<SaleDetailRequest> Detalles { get; set; }

        public CreateSaleRequest()
        {
            Detalles = new List<SaleDetailRequest>();
        }
    }

    public class SaleDetailRequest
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
    #endregion
}
