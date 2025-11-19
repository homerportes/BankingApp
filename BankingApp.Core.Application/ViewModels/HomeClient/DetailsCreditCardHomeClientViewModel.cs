

using BankingApp.Core.Domain.Common.Enums;

namespace BankingApp.Core.Application.ViewModels.HomeClient
{
    public class DetailsCreditCardHomeClientViewModel
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public decimal AmountSpent { get; set; }

        public required string MerchantName { get; set; }
        public required OperationStatus Status { get; set; }

    }
}
