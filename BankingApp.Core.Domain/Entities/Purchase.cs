using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BankingApp.Core.Domain.Entities
{

    // Consumo
    public class Purchase
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime DateTime { get; set; }
        public decimal AmountSpent { get; set; }

        //nombre del comercio donde se realizó el consumo; si se trata de un avance de efectivo, en lugar del nombre del comercio debe aparecer el texto "AVANCE
        public required string MerchantName {  get; set; }
        public string ?CardId { get; set; }

        public CreditCard? CreditCard { get; set; }
    }
}
