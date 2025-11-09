using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Installment
{
    public class InstallmentDto
    {
        [JsonProperty("Cuota")]

        public required int Number { get; set; }

        [JsonProperty("FechaPago")]
        public required DateOnly PayDate { get; set; }
        [JsonProperty("Valor")]

        public required Decimal Value { get; set; }

        [JsonProperty("Pagada")]

        public bool IsPaid { get; set; } = false;
        [JsonProperty("atrasada")]

        public bool IsDelinquent { get; set; } = false;

    }
}
