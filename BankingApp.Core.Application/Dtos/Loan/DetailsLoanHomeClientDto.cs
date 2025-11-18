

namespace BankingApp.Core.Application.Dtos.Loan
{
    public class DetailsLoanHomeClientDto
    {


        public Guid IdLoan  { get; set; }
        public DateOnly PayDate { get; set; }
        public decimal Value { get; set; }
        public bool IsPaid { get; set; }
        public bool IsDelinquent { get; set; } = false;


    }

}
