using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.HomeClient
{
    public class DetailsLoanHomeClientViewModel
    {

        public Guid IdLoan { get; set; }
        public DateOnly PayDate { get; set; }
        public decimal Value { get; set; }
        public bool IsPaid { get; set; }
        public bool IsDelinquent { get; set; } = false;


    }
}
