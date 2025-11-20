using BankingApp.Core.Domain.Common.Enums;
using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.CreditCard
{
    public class CreditCardDto
    {
        public int Id { get; set; }

        [JsonProperty("numeroTarjeta")]
        public required string Number { get; set; }

        [JsonProperty("clienteId")]
        public required string ClientId { get; set; }

        [JsonProperty("nombreCliente")]
        public string? ClientName { get; set; }

        [JsonProperty("limiteCredito")]
        public decimal CreditLimitAmount { get; set; }

        [JsonProperty("fechaExpiracion")]
        public string? ExpirationDate { get; set; }

        [JsonProperty("totalAdeudado")]
        public decimal TotalAmountOwed { get; set; }

        [JsonProperty("estado")]
        public CardStatus Status { get; set; }

        public string? AdminId { get; set; }

        public string ? CVC {  get; set; }
    }
}
