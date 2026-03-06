using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiAegis.Models;

namespace ApiAegis.DAO
{
    public class DashboardDao
    {
        #region Properties & Constructor
        private readonly string _connectionString;

        public DashboardDao(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no ha sido inicializada.");
        }
        #endregion

        #region Dashboard Stats Processing
        public DashboardStatsModel ObtenerEstadisticas()
        {
            DashboardStatsModel stats = new DashboardStatsModel();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ObtenerDashboardStats", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.VentasHoy = Convert.ToDecimal(reader["VentasHoy"]);
                            stats.AlertasStock = Convert.ToInt32(reader["AlertasStock"]);
                            stats.ProveedoresActivos = Convert.ToInt32(reader["ProveedoresActivos"]);
                        }
                    }
                }
            }

            return stats;
        }
        #endregion
    }
}
