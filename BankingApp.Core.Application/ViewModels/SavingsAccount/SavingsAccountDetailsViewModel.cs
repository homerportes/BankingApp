namespace BankingApp.Core.Application.ViewModels.SavingsAccount
{
    public class SavingsAccountDetailsViewModel
    {
        public SavingsAccountViewModel Account { get; set; } = new();
        public List<TransactionViewModel> Transactions { get; set; } = new();
    }
}
