

namespace BankingApp.Core.Application.Dtos.Loan
{
    public class DataLoanHomeClientDto
    {

        public Guid Id { get; set; }
    
       public string? Number { get; set; }
        public decimal LoanedAmountTotal { get; set; }
        public required decimal OutstandingBalance { get; set; }
        public int installmentsTotalAmount { get; set; }
        public int installmentsTotalAmountPaid { get; set; }
 
        public required decimal InterestRate { get; set; } 
     
        public required int LoanTermInMonths { get; set; }

        public bool IsDelinquent { get; set; } = false;




    }

}
