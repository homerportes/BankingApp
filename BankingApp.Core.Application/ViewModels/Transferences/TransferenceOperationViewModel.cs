using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.Transferences
{
    public class TransferenceOperationViewModel
    {
        public bool? HasError { get; set; }
        public string? Error { get; set; }

        [Required(ErrorMessage = "La cuenta de origen es requerida")]
        public string? AccountNumberFrom { get; set; }

        [Required(ErrorMessage = "La cuenta de destino es requerida")]
        public string? AccountNumberTo { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(1, 999999999999999999, ErrorMessage = "El monto debe ser mayor que cero")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public required List<string> AvailableAccounts { get; set; }
        public string? Message { get; set; }
    }
}
