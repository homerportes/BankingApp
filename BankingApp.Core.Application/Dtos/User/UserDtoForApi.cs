using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UserDtoForApi
    {
        [JsonProperty("usuario")]

        public required string UserName { get; set; }
        [JsonProperty("correo")]

        public required string Email { get; set; }
        [JsonProperty("nombre")]

        public required string Name { get; set; }
        [JsonProperty("apellido")]

        public required string LastName { get; set; }
        [JsonProperty("cedula")]

        public required string DocumentIdNumber { get; set; }
        [JsonProperty("rol")]

        public required string Role { get; set; }
        [JsonProperty("estado")]
        public required string Status { get; set; }


    }
}
