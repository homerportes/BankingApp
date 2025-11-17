using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.Loan
{
    public class TermPageViewModel
    {
        [Required(ErrorMessage = "El plazo del préstamo es requerido")]
        public int TermInMonths { get; set; }

        [Required(ErrorMessage = "El cliente es requerido")]
        public string? ClientId { get; set; }

        [Required(ErrorMessage = "El monto del préstamo es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La tasa del préstamo es requerida")]
        [Range(0, double.MaxValue, ErrorMessage = "La tasa debe ser mayor o igual a 0")]
        public decimal AnnualInterestRate { get; set; }
    }
}
