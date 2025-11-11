using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<CreditCard?> GetByNumberAsync(string cardNumber);
        Task<List<CreditCard>> GetByClientIdAsync(string clientId);
        Task<bool> CardNumberExistsAsync(string cardNumber);
    }
}
