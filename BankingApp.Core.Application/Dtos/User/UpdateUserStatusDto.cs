using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UpdateUserStatusDto
    {
        [JsonProperty("activo")]
        public required bool IsActive { get; set; }
    }
}
