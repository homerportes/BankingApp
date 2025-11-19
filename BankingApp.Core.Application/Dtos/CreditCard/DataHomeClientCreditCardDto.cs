

namespace BankingApp.Core.Application.Dtos.CreditCard
{
    public class DataHomeClientCreditCardDto
    {


        public int Id { get; set; }
        public string? Number { get; set; }
        public decimal CreditLimitAmount { get; set; }
        public DateTime ExpirationDate { get; set; }
        public required decimal TotalAmountOwed { get; set; }


    }


}
