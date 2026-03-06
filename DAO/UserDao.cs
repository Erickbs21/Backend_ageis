using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiAegis.Models;

namespace ApiAegis.DAO
{
    public class UserDao
    {
        private readonly string _connectionString;

        public UserDao(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no ha sido inicializada.");
        }

        public UserModel AutenticarUsuario(string username, string password)
        {
            UserModel user = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_AutenticarUsuario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password); // En producción verificar Hash aquí o en el SP

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new UserModel(
                                id: Convert.ToInt32(reader["Id"]),
                                name: reader["Nombre"].ToString(),
                                username: reader["Username"].ToString(),
                                role: reader["Role"].ToString()
                            );
                        }
                    }
                }
            }

            return user;
        }

        // Método genérico basado en la firma que proporcionaste para logueo o bitácoras adicionales (opcional en base al requerimiento base)
        public string AutenticarMedios(string codUsuario, string clave, string UID, string dispositivo, string tipoDispositivo, string versionSO, string versionApp, string tipoLogin,
        string Comercio, string Agencia, string Usuario, string Password)
        {
             // Lógica extendida si se requiere en un futuro
             // Esta firma coincide con tu ejemplo de referencia
             throw new NotImplementedException("Este método es parte de la retrocompatibilidad referencial mostrada.");
        }
    }
}
