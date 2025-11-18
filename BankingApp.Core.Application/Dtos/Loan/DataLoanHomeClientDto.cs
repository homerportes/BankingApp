

namespace BankingApp.Core.Application.Dtos.Loan
{
    public class DataLoanHomeClientDto
    {

        public Guid Id { get; set; }
        //total prestado
        public decimal LoanedAmountTotal { get; set; }
        //moto que se debe actualmente
        public required decimal OutstandingBalance { get; set; }
        //cantidad de cuotas total
        public int installmentsTotalAmount { get; set; }
        //cantidad de cuotas pagadas
        public int installmentsTotalAmountPaid { get; set; }

        //tasa de interes aplicada
        public required decimal InterestRate { get; set; } 
        //plazo expresados en meses
        public required int LoanTermInMonths { get; set; }

        //esta al dia o en mora
        public bool IsDelinquent { get; set; } = false;




    }

}
