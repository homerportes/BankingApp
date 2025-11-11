using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Commerce
{
    public class CreateCommerceDto
    {

        [JsonProperty("Nombre")]

        public required string Name { get; set; }
        [JsonProperty("Descripcion")]

        public required string Description { get; set; }

        public required string Logo { get; set; }
    }
}
