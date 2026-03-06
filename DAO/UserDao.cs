using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiAegis.Models;

namespace ApiAegis.DAO
{
    public class UserDao
    {
        #region Properties & Constructor
        private readonly string _connectionString;

        public UserDao(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no ha sido inicializada.");
        }
        #endregion

        #region Authentication
        public UserModel AutenticarUsuario(string username, string password)
        {
            UserModel user = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_AutenticarUsuario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

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
        
        public string AutenticarMedios(string codUsuario, string clave, string UID, string dispositivo, string tipoDispositivo, string versionSO, string versionApp, string tipoLogin, string Comercio, string Agencia, string Usuario, string Password)
        {
             throw new NotImplementedException("Este método es parte de la retrocompatibilidad referencial mostrada.");
        }
        #endregion

        #region User Management
        public List<UserModel> ObtenerUsuarios()
        {
            List<UserModel> usuarios = new List<UserModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ObtenerUsuarios", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(new UserModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Nombre"].ToString(),
                                Username = reader["Username"].ToString(),
                                Role = reader["Role"].ToString(),
                                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                                Activo = Convert.ToBoolean(reader["Activo"])
                            });
                        }
                    }
                }
            }

            return usuarios;
        }

        public int CrearUsuario(UserModel usuario, string password)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_CrearUsuario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Nombre", usuario.Name);
                    command.Parameters.AddWithValue("@Username", usuario.Username);
                    command.Parameters.AddWithValue("@PasswordHash", password); // En producción se debe guardar un Hash
                    command.Parameters.AddWithValue("@Role", usuario.Role);

                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public bool ResetearPassword(int idUsuario, string nuevoPassword)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_ResetearPasswordUsuario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    command.Parameters.AddWithValue("@NuevoPasswordHash", nuevoPassword); // En producción verificar Hash aquí

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        #endregion
    }
}
