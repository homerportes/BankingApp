using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<bool> AccountExists(string accountNumber);
        Task<int> CountSavingAccountsByUserIds(HashSet<string> userIds);

       
    }
}
