using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Entities
{
    public class Loan
    {
        public Guid Id { get; set; }
        public required string ClientId { get; set; }


        //Total del prestamo
        public decimal TotalLoanAmount { get; set; }

        //Total de cuotas del  prestamo
        public required int TotalInstallmentsCount { get; set; }

        //Total de cuotas  pagadas del  prestamo
        public required int PaidInstallmentsCount { get; set; }


        //Monto restante a pagar  prestamo
        public required decimal OutstandingBalance { get; set; }

        //Tasa aplicada
        public required  decimal InterestRate { get; set; }
        //Plazo del prestamo en meses
        public required int LoanTermInMonths { get; set; }

        public  required LoanStatus Status { get; set; }

    }
}
