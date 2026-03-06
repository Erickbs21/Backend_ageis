using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiAegis.Models;

namespace ApiAegis.DAO
{
    public class ProviderDao
    {
        #region Properties & Constructor
        private readonly string _connectionString;

        public ProviderDao(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no ha sido inicializada.");
        }
        #endregion

        #region Provider Management
        public List<ProviderModel> ObtenerProveedores()
        {
            List<ProviderModel> proveedores = new List<ProviderModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ObtenerProveedores", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            proveedores.Add(new ProviderModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nombre = reader["Nombre"].ToString(),
                                NitRfc = reader["NitRfc"].ToString(),
                                Telefono = reader["Telefono"].ToString()
                            });
                        }
                    }
                }
            }

            return proveedores;
        }

        public int CrearProveedor(CreateProviderRequest request)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_CrearProveedor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Nombre", request.Nombre);
                    command.Parameters.AddWithValue("@NitRfc", request.NitRfc);
                    command.Parameters.AddWithValue("@Telefono", request.Telefono ?? (object)DBNull.Value);

                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public bool ActualizarProveedor(int id, CreateProviderRequest request)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ActualizarProveedor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Nombre", request.Nombre);
                    command.Parameters.AddWithValue("@NitRfc", request.NitRfc);
                    command.Parameters.AddWithValue("@Telefono", request.Telefono ?? (object)DBNull.Value);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        #endregion
    }
}
