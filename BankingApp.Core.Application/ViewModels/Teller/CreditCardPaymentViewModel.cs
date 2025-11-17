using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.Teller
{
    /// <summary>
    /// ViewModel para realizar pagos a tarjetas de crédito desde el cajero
    /// </summary>
    public class CreditCardPaymentViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido")]
        [Display(Name = "Número de cuenta origen")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Display(Name = "Monto a pagar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El número de tarjeta es requerido")]
        [Display(Name = "Número de tarjeta de crédito")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
        public string CardNumber { get; set; } = string.Empty;

        // Propiedades para la confirmación
        public string CardHolderName { get; set; } = string.Empty;
        public decimal CurrentDebt { get; set; }
        public decimal ActualAmountToPay { get; set; }
        public bool ShowConfirmation { get; set; }

        // Propiedades para manejo de errores
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
