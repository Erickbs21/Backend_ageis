using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiAegis.Models;

namespace ApiAegis.DAO
{
    public class SaleDao
    {
        #region Properties & Constructor
        private readonly string _connectionString;

        public SaleDao(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no ha sido inicializada.");
        }
        #endregion

        #region Sale Processing
        public int CrearVentaCompleta(CreateSaleRequest request)
        {
            // Calcula el total general
            decimal totalGeneral = 0;
            foreach (var detalle in request.Detalles)
            {
                totalGeneral += (detalle.Cantidad * detalle.PrecioUnitario);
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Solo para estructurar; el SP AgregarDetalleVenta ya tiene un BEGIN TRY / BEGIN TRAN interno.
                // Aquí, el manejo es a nivel aplicación conectando a DB.
                connection.Open();

                int idNuevaVenta = 0;

                // 1. Crear Venta Cabecera
                using (SqlCommand cmdCabecera = new SqlCommand("sp_CrearVenta", connection))
                {
                    cmdCabecera.CommandType = CommandType.StoredProcedure;
                    cmdCabecera.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmdCabecera.Parameters.AddWithValue("@Total", totalGeneral);
                    
                    SqlParameter outIdVenta = new SqlParameter("@IdNuevaVenta", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmdCabecera.Parameters.Add(outIdVenta);

                    cmdCabecera.ExecuteNonQuery();
                    idNuevaVenta = Convert.ToInt32(outIdVenta.Value);
                }

                if (idNuevaVenta <= 0)
                {
                    throw new Exception("No se pudo generar la cabecera de la venta.");
                }

                // 2. Insertar Detalles y Descontar Stock
                foreach (var detalle in request.Detalles)
                {
                    decimal subtotal = detalle.Cantidad * detalle.PrecioUnitario;

                    using (SqlCommand cmdDetalle = new SqlCommand("sp_AgregarDetalleVenta", connection))
                    {
                        cmdDetalle.CommandType = CommandType.StoredProcedure;
                        cmdDetalle.Parameters.AddWithValue("@IdVenta", idNuevaVenta);
                        cmdDetalle.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                        cmdDetalle.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                        cmdDetalle.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
                        cmdDetalle.Parameters.AddWithValue("@Subtotal", subtotal);
                        
                        cmdDetalle.ExecuteNonQuery(); // Si falla por stock, lanzará una SqlException
                    }
                }

                return idNuevaVenta;
            }
        }
        #endregion

        #region Information Retrieval
        public List<SaleModel> ObtenerHistorialVentas(DateTime? fechaInicio, DateTime? fechaFin, int? idUsuario)
        {
            List<SaleModel> ventas = new List<SaleModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ObtenerHistorialVentas", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FechaInicio", fechaInicio ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FechaFin", fechaFin ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IdUsuario", idUsuario ?? (object)DBNull.Value);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ventas.Add(new SaleModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                Cajero = reader["Cajero"].ToString(),
                                Total = Convert.ToDecimal(reader["Total"]),
                                FechaVenta = Convert.ToDateTime(reader["FechaVenta"]),
                                Estado = reader["Estado"].ToString()
                            });
                        }
                    }
                }
            }

            return ventas;
        }

        public List<SaleDetailModel> ObtenerTicketVenta(int idVenta)
        {
            List<SaleDetailModel> detalles = new List<SaleDetailModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ObtenerTicketVenta", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdVenta", idVenta);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detalles.Add(new SaleDetailModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IdProducto = Convert.ToInt32(reader["IdProducto"]),
                                Producto = reader["Producto"].ToString(),
                                CodigoBarras = reader["CodigoBarras"].ToString(),
                                Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                                Subtotal = Convert.ToDecimal(reader["Subtotal"])
                            });
                        }
                    }
                }
            }

            return detalles;
        }
        #endregion
    }
}
