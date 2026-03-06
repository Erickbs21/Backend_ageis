using System;

namespace ApiAegis.Models
{
    #region ProductModel
    public class ProductModel
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; }
    }
    #endregion

    #region Requests DTOs
    public class CreateProductRequest
    {
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
    }

    public class UpdateProductRequest
    {
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public int StockMinimo { get; set; }
    }
    #endregion
}
