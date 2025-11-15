using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Stats
{
    public class AdminDashboardStatsDto
    {
        public int TotalTransactionsCount { get; set; }
        public int DayPaysCount { get; set; }
        public int TotalPaysCount { get; set; }
        public int TotalInactiveClientsCount { get; set; }
        public int TotalActiveClientsCount { get; set; }
        public int TotalAsignedProductsCount { get; set; }
        public int TotalCurrentLoansCount { get; set; }
        public int TotalActiveCreditCardsCount { get; set; }
        public int TotalSavingAccountsCount { get; set; }
        public decimal AverageClientsDebt { get; set; }
        public int TotalIssuedCreditCardsCount { get; internal set; }
        public int TotalClientCreditCardsCount { get; internal set; }
    }
}
