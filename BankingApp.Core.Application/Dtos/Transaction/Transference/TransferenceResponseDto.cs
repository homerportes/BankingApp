using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Transaction.Transference
{
    public class TransferenceResponseDto
    {
        public bool IsSuccessful { get; set; } = true;
        public required string Message { get; set; }    
    }
}
