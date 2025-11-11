using BankingApp.Core.Application.Dtos.Installment;
using Newtonsoft.Json;


namespace BankingApp.Core.Application.Dtos.Loan
{
    public class DetailedLoanDto
    {
        [JsonProperty("prestamoId")]

        public required string LoadId { get; set; }

        [JsonProperty("TablaAmortizacion")]
        public required List<InstallmentDto> Installments { get; set; }
    }
}
