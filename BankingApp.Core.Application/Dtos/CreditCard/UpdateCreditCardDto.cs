using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.CreditCard
{
    public class UpdateCreditCardDto
    {
        [Required(ErrorMessage = "El límite de crédito es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor a 0")]
        [JsonProperty("limiteCredito")]
        public decimal CreditLimitAmount { get; set; }
    }
}
