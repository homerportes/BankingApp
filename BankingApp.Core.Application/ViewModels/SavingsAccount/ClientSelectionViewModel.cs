namespace BankingApp.Core.Application.ViewModels.SavingsAccount
{
    public class ClientSelectionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
        public bool IsSelected { get; set; }
    }
}
