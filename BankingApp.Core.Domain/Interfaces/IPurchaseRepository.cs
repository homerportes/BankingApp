using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface IPurchaseRepository : IGenericRepository<Purchase>
    {
        Task<List<Purchase>> GetByCardNumberAsync(string cardNumber);

        Task<bool> ExistsAsync(string cardNumber, decimal amountSpent, string merchantName);
    }
}
