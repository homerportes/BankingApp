using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Account
{
    public class DataAccountaHomeClientDto
    {

        public int Id { get; set; }
        public string? Number { get; set; }
        public decimal Banlace {  get; set; }
        public AccountType Type { get; set; }

    }
}
