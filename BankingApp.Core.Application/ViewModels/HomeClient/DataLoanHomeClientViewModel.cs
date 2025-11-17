using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.HomeClient
{
    public class DataLoanHomeClientViewModel
    {
        public Guid Id { get; set; }
        public decimal LoanedAmountTotal { get; set; }
        public required decimal OutstandingBalance { get; set; }
        public int installmentsTotalAmount { get; set; }
        public int installmentsTotalAmountPaid { get; set; }
        public required decimal InterestRate { get; set; }
        public required int LoanTermInMonths { get; set; }
        public bool IsDelinquent { get; set; } = false;

    }
}
