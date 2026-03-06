using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiAegis.Models;

namespace ApiAegis.DAO
{
    public class CashCutDao
    {
        #region Properties & Constructor
        private readonly string _connectionString;

        public CashCutDao(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no ha sido inicializada.");
        }
        #endregion

        #region CashCut Processing
        public CashCutModel ObtenerEstadoCaja()
        {
            CashCutModel caja = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_EstadoCaja", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            caja = new CashCutModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                FechaApertura = Convert.ToDateTime(reader["FechaApertura"]),
                                SaldoInicial = Convert.ToDecimal(reader["SaldoInicial"]),
                                TotalVentas = Convert.ToDecimal(reader["TotalVentas"]),
                                TotalEgresos = Convert.ToDecimal(reader["TotalEgresos"]),
                                Estado = reader["Estado"].ToString()
                            };
                        }
                    }
                }
            }

            return caja;
        }

        public void AbrirCaja(int idUsuario, decimal saldoInicial)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_AbrirCaja", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    command.Parameters.AddWithValue("@SaldoInicial", saldoInicial);

                    connection.Open();
                    command.ExecuteNonQuery(); // Puede arrojar SqlException si la caja ya está abierta
                }
            }
        }

        public decimal CerrarCaja(int id)
        {
            decimal saldoFinal = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_CerrarCaja", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();
                    object result = command.ExecuteScalar(); // Devuelve el SaldoFinal
                    
                    if (result != null && result != DBNull.Value)
                    {
                        saldoFinal = Convert.ToDecimal(result);
                    }
                }
            }

            return saldoFinal;
        }
        #endregion
    }
}
