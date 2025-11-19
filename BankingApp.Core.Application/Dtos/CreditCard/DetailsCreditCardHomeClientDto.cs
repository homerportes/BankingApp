

using BankingApp.Core.Domain.Common.Enums;

namespace BankingApp.Core.Application.Dtos.CreditCard
{
    public class DetailsCreditCardHomeClientDto
    {

        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public decimal AmountSpent { get; set; }

        //nombre del comercio donde se realizó el consumo; si se trata de un avance de efectivo, en lugar del nombre del comercio debe aparecer el texto "AVANCE
        public required string MerchantName { get; set; }
        public required OperationStatus Status { get; set; }


    }
}
