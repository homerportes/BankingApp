using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.Dtos.User
{
    public class GetResetTokenDto
    {
        [JsonProperty("userName")]
        [Required]
        public required string UserName { get; set; }
    }
}
