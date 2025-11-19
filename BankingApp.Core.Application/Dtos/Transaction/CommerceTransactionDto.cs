using BankingApp.Core.Domain.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Transaction
{
    public class CommerceTransactionDto
    {
        [JsonProperty("Monto")]
        public decimal Amount { get; set; }
        [JsonProperty("Fecha")]


        public DateTime DateTime { get; set; }

        [JsonProperty("Tipo")]

        public required string Type { get; set; }

        [JsonProperty("Beneficiario")]

        public required string Beneficiary { get; set; }
        [JsonProperty("Origen")]

        public required string Origin  { get; set; }
        [JsonProperty("Estado")]

        public required string Status { get; set; }

    }
}
