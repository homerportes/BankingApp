using BankingApp.Core.Domain.Common.Enums;

namespace BankingApp.Core.Application.ViewModels.CreditCard
{
    public class PurchaseViewModel
    {
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public required string CommerceName { get; set; }
        public string? Description { get; set; }
        public OperationStatus Status { get; set; }
    }
}
