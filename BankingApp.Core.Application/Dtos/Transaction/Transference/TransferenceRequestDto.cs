using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Transaction.Transference
{
    public class TransferenceRequestDto
    {
       

        public  required string AccountNumberFrom { get; set; }

        public required string AccountNumberTo { get; set; }

        public  decimal Amount { get; set; }

    }
}
