

namespace BankingApp.Core.Application.ViewModels.Dashboards
{
    public class AdminDashboardViewModel
    {
       public int TotalTransactionsCount { get; set; }
        public int DayTransactionsCount { get; set; }
        public int DayPaysCount { get; set; }
        public int TotalPaysCount { get; set; }
        public int TotalInactiveClientsCount { get; set; }
        public int TotalActiveClientsCount { get; set; }
        public int TotalAsignedProductsCount { get; set; }
        public int TotalCurrentLoansCount { get; set; }
        public int TotalActiveCreditCardsCount { get; set; }
        public int TotalSavingAccountsCount { get; set; }
        public decimal AverageClientsDebt { get; set; }



    }
}
