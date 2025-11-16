namespace BankingApp.Core.Application.ViewModels.Teller
{
    /// <summary>
    /// ViewModel para el dashboard del cajero con indicadores del día
    /// </summary>
    public class TellerDashboardViewModel
    {
        /// <summary>
        /// Número total de transacciones realizadas por el cajero en el día
        /// </summary>
        public int TotalTransactionsToday { get; set; }

        /// <summary>
        /// Número total de pagos (TC + Préstamos) realizados por el cajero en el día
        /// </summary>
        public int TotalPaymentsToday { get; set; }

        /// <summary>
        /// Número total de depósitos realizados por el cajero en el día
        /// </summary>
        public int TotalDepositsToday { get; set; }

        /// <summary>
        /// Número total de retiros realizados por el cajero en el día
        /// </summary>
        public int TotalWithdrawalsToday { get; set; }
    }
}
