using System;

namespace ApiAegis.Models
{
    #region UserModel
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }

        public UserModel() { }

        public UserModel(int id, string name, string username, string role)
        {
            Id = id;
            Name = name;
            Username = username;
            Role = role;
        }
    }
    #endregion

    #region User Requests DTOs
    public class CreateUserRequest
    {
        public string Nombre { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; }
    }
    #endregion
}
