using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Transaction
{
    public class ValidateTotalAmountOwedResponseDto
    {

        public int AccountId { get; set; }
        public bool HasError { get; set; }
        public string? Error { get; set; }   

    }
}
