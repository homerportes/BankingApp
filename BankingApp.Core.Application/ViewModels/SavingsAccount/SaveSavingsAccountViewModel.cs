using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.SavingsAccount
{
    public class SaveSavingsAccountViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El balance inicial es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El balance inicial no puede ser negativo")]
        [Display(Name = "Balance Inicial")]
        public decimal InitialBalance { get; set; }

        // Información del cliente seleccionado (para mostrar en la vista)
        public string? ClientName { get; set; }
        public string? ClientDocumentId { get; set; }
        public decimal TotalDebt { get; set; }
    }
}
