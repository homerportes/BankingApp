using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.Dtos.User
{
    public class ConfirmAccountDto
    {
        [JsonProperty("userId")]
        [Required]
        public required string UserId { get; set; }

        [JsonProperty("token")]
        [Required]
        public required string Token { get; set; }
    }
}
