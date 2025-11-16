using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Payment
{
    public class PaymentResultDto
    {
        public bool IsInternalError { get; set; }
        public bool CardIsValid { get; set; }
        public bool CardExists { get; set; }
        public string? Message { get;  set; }
        public bool CommerceIsValid { get;  set; }
        public bool CardHasEnoughFunds { get; internal set; }
        public bool Continue { get; internal set; }
        public bool IsCompleted { get; internal set; }
    }
}
