using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.Dtos.User
{
    public class ResetPasswordApiDto
    {
        [JsonProperty("userId")]
        [Required]
        public required string UserId { get; set; }

        [JsonProperty("token")]
        [Required]
        public required string Token { get; set; }

        [JsonProperty("password")]
        [Required]
        public required string Password { get; set; }

        [JsonProperty("confirmPassword")]
        [Required]
        public required string ConfirmPassword { get; set; }
    }
}
