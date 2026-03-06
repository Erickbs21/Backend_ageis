using System;
using System.Text.Json.Serialization;

namespace ApiAegis.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("user")]
        public UserModel User { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        public LoginResponse()
        {
        }

        public LoginResponse(UserModel user, string token)
        {
            User = user;
            Token = token;
        }
    }
}
