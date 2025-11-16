using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Payment
{
    public class PaymentRequestDto
    {

        public required string CardNumber { get; set; }
        public required int MonthExpirationCard { get; set; }
        public required int YearExpirationCard { get; set; }
        public required string Cvc { get; set; }

        public required decimal TransactionAmount { get; set; }

    }
}
