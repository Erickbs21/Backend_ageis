using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiAegis.Models;

namespace ApiAegis.DAO
{
    public class ProductDao
    {
        #region Properties & Constructor
        private readonly string _connectionString;

        public ProductDao(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no ha sido inicializada.");
        }
        #endregion

        #region Product Management
        public List<ProductModel> ObtenerProductos()
        {
            List<ProductModel> productos = new List<ProductModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ObtenerProductos", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productos.Add(new ProductModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CodigoBarras = reader["CodigoBarras"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Categoria = reader["Categoria"].ToString(),
                                Precio = Convert.ToDecimal(reader["Precio"]),
                                Stock = Convert.ToInt32(reader["Stock"]),
                                StockMinimo = Convert.ToInt32(reader["StockMinimo"])
                            });
                        }
                    }
                }
            }

            return productos;
        }

        public int CrearProducto(CreateProductRequest producto)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_CrearProducto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CodigoBarras", producto.CodigoBarras);
                    command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                    command.Parameters.AddWithValue("@Descripcion", producto.Descripcion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Categoria", producto.Categoria ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Precio", producto.Precio);
                    command.Parameters.AddWithValue("@Stock", producto.Stock);
                    command.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);

                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public bool ActualizarProducto(int id, UpdateProductRequest request)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ActualizarProducto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Precio", request.Precio);
                    command.Parameters.AddWithValue("@Descripcion", request.Descripcion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@StockMinimo", request.StockMinimo);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool EliminarProducto(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_EliminarProducto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        #endregion
    }
}
