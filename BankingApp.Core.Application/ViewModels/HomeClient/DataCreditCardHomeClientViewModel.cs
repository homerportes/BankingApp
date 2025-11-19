using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.HomeClient
{
    public class DataCreditCardHomeClientViewModel
    {

        public int Id { get; set; }
        public string? Number { get; set; }
        public decimal CreditLimitAmount { get; set; }
        public DateTime ExpirationDate { get; set; }
        public required decimal TotalAmountOwed { get; set; }

    }
}
