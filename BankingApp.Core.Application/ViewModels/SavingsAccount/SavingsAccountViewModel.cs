using BankingApp.Core.Domain.Common.Enums;

namespace BankingApp.Core.Application.ViewModels.SavingsAccount
{
    public class SavingsAccountViewModel
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public AccountType Type { get; set; }
        public AccountStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdminId { get; set; }

        // Propiedad calculada para mostrar el tipo en español
        public string TypeDisplay => Type == AccountType.PRIMARY ? "Principal" : "Secundaria";
        
        // Propiedad calculada para mostrar el estado en español
        public string StatusDisplay => Status == AccountStatus.ACTIVE ? "Activa" : "Cancelada";
    }
}
