using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Common.Enums
{
    public enum DescriptionTransaction
    {
        Transaccion_Express = 1,
        Transaccion_A_Prestamo = 2,
        Trasaccion_A_Tarjeta = 3,
        Transaccion_A_Beneficiario = 4,
        Cash_Advance = 5,

        DEPOSIT = 6,
        WITHDRAWAL = 7,
        CREDITCARDPAYMENT = 8,
        LOANPAYMENT = 9,
        TRANSFER = 10
    }
}
