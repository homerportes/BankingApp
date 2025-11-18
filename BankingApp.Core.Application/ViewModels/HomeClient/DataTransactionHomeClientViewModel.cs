using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.HomeClient
{
    public class DataTransactionHomeClientViewModel
    {

        public Guid Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public TransactionType Type { get; set; }
        public string? Beneficiary { get; set; }
        public string? Origin { get; set; }
        public OperationStatus Status { get; set; }
        public DescriptionTransaction Description { get; set; }

    }
}
