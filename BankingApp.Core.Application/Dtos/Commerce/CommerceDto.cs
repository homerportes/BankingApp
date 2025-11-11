using Newtonsoft.Json;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace BankingApp.Core.Application.Dtos.Commerce
{
    public class CommerceDto
    {
        
        public int Id { get; set; }

        [JsonIgnoreAttribute]

        public string? UserId { get; set; }

        [JsonProperty("Estado")]

        [JsonIgnoreAttribute]
        public required string Status { get; set; }
        [JsonProperty("Nombre")]

        public required string Name { get; set; }
        [JsonProperty("Descripcion")]

        public required string Description { get; set; }

        public required string Logo { get; set; }
        [JsonIgnoreAttribute]

        public bool IsActive { get; set; }
    }
}
