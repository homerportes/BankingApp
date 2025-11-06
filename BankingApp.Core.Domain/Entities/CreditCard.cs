using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Entities
{
    public class CreditCard
    {
       public required int Id { get; set; }
        //16 UNIQUE DIGITS generated
       public required string Number { get; set; }
       public required string ClientId { get; set; }

        public decimal CreditLimitAmount { get; set; }

        // Se guarda usando el ultimo dia del mes y se imprime en formato MM/YY
        public DateTime ExpirationDate { get; set; }
        public  required decimal TotalAmountOwed { get; set; }


        //Se generan 3 digitos y se cifran con SHA-256 
        public required string CVC {  get; set; }


        public  required CardStatus Status { get; set; }


        //Admin que hizo la asignacion
        public required string AdminId { get; set; }


        public ICollection<Purchase>? Purchases { get; set; }

    }
}
