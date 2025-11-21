using BankingApp.Core.Domain.Common.Enums;

namespace BankingApp.Core.Application.ViewModels.CreditCard
{
    public class CreditCardViewModel
    {
        public int Id { get; set; }
        public required string CardNumber { get; set; }
        public required string ClientId { get; set; }
        public string? ClientName { get; set; }
        public decimal CreditLimit { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal CurrentDebt { get; set; }
        public decimal AvailableCredit => CreditLimit - CurrentDebt;
        public CardStatus CardStatus { get; set; }
        public string? AdminId { get; set; }
        public string? CVC { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
