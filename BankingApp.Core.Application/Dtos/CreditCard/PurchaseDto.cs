using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.CreditCard
{
    public class PurchaseDto
    {
        [JsonProperty("fecha")]
        public DateTime DateTime { get; set; }

        [JsonProperty("monto")]
        public decimal AmountSpent { get; set; }

        [JsonProperty("comercio")]
        public required string MerchantName { get; set; }

        [JsonProperty("estado")]
        public required string Status { get; set; }
    }
}
