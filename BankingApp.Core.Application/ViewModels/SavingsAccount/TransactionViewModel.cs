using BankingApp.Core.Domain.Common.Enums;

namespace BankingApp.Core.Application.ViewModels.SavingsAccount
{
    public class TransactionViewModel
    {
        public Guid Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Beneficiary { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public OperationStatus Status { get; set; }

        // Propiedades calculadas para mostrar en español
        public string TypeDisplay => Type == TransactionType.DEBIT ? "DÉBITO" : "CRÉDITO";
        public string StatusDisplay => Status == OperationStatus.APPROVED ? "APROBADA" : "RECHAZADA";
    }
}
