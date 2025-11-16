using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.Teller
{
    /// <summary>
    /// ViewModel para realizar transacciones entre cuentas de terceros desde el cajero
    /// </summary>
    public class ThirdPartyTransactionViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido")]
        [Display(Name = "Número de cuenta origen")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        public string SourceAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Display(Name = "Monto de la transacción")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        [Display(Name = "Número de cuenta destino")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        public string DestinationAccountNumber { get; set; } = string.Empty;

        // Propiedades para la confirmación
        public string DestinationAccountHolderName { get; set; } = string.Empty;
        public bool ShowConfirmation { get; set; }
    }
}
