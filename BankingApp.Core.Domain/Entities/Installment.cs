using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Entities
{
    public class Installment
    {
       public required int Id { get; set; }

        public required DateOnly PayDate { get; set; }

        public required Decimal Value { get; set; }
        public  bool IsPaid  { get; set; }=false;
        public bool IsDelinquent { get; set; }=false;

        public  required Guid LoanId { get; set; }
        public Loan ? Loan { get; set; }
        public int Number { get; set; }
        public bool IsModified { get; set; }
    }
}
