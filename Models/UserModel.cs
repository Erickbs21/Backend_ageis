using System;

namespace ApiAegis.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        public UserModel()
        {
        }

        public UserModel(int id, string name, string username, string role)
        {
            Id = id;
            Name = name;
            Username = username;
            Role = role;
        }
    }
}
