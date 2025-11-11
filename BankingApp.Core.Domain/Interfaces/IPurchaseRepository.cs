using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface IPurchaseRepository : IGenericRepository<Purchase>
    {
        Task<List<Purchase>> GetByCardNumberAsync(string cardNumber);
    }
}
