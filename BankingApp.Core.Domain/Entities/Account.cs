using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Entities
{
    public class Account
    {

        public required string Id { get; set; }


        public required  string ClientId { get; set; }    
        public Decimal Balance { get; set; }
        public AccountType Type { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
