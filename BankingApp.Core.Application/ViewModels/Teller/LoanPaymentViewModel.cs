using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.Teller
{
    /// <summary>
    /// ViewModel para realizar pagos a préstamos desde el cajero
    /// </summary>
    public class LoanPaymentViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido")]
        [Display(Name = "Número de cuenta origen")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Display(Name = "Monto a pagar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El número de préstamo es requerido")]
        [Display(Name = "Número de préstamo")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de préstamo debe tener 9 dígitos")]
        public string LoanNumber { get; set; } = string.Empty;

        // Propiedades para la confirmación
        public string LoanHolderName { get; set; } = string.Empty;
        public decimal RemainingBalance { get; set; }
        public bool ShowConfirmation { get; set; }
    }
}
