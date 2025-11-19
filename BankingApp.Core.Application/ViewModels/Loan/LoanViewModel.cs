using BankingApp.Core.Domain.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Loan
{
    public class LoanViewModel
    {
        public Guid Id { get; set; }

        public required string ClientId { get; set; }


        public required string ClientName { get; set; }


        public required string ClientDocumentIdNumber { get; set; }


        public required string PublicId { get; set; }



        public decimal Amount { get; set; }



        public required int TotalInstallmentsCount { get; set; }



        public required int PaidInstallmentsCount { get; set; }
        public required bool Isdelinquent { get; set; }




        public required decimal OutstandingBalance { get; set; }


        public required decimal InterestRate { get; set; }


        public required int LoanTermInMonths { get; set; }



        public required string LoanStatus { get; set; }


        public required LoanStatus Status { get; set; }
    }
}
