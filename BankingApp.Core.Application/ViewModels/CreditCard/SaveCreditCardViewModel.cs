using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.CreditCard
{
    public class SaveCreditCardViewModel
    {
        public int? Id { get; set; }

        public string? ClientId { get; set; }

        [Required(ErrorMessage = "El límite de crédito es requerido")]
        [Range(1000, 1000000, ErrorMessage = "El límite debe estar entre $1,000 y $1,000,000")]
        [Display(Name = "Límite de Crédito")]
        public decimal CreditLimit { get; set; }
    }
}
