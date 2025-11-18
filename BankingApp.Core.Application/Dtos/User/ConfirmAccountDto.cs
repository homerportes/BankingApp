using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.Dtos.User
{
    public class ConfirmAccountDto
    {
     

        [JsonProperty("token")]
        [Required]
        public required string Token { get; set; }
    }
}
