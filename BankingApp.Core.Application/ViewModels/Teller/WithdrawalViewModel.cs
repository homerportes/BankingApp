using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.Teller
{
    /// <summary>
    /// ViewModel para realizar retiros desde el cajero
    /// </summary>
    public class WithdrawalViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido")]
        [Display(Name = "Número de cuenta origen")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Display(Name = "Monto a retirar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Amount { get; set; }

        // Propiedades para la confirmación
        public string AccountHolderName { get; set; } = string.Empty;
        public bool ShowConfirmation { get; set; }
    }
}
