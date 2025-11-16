using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<bool> AccountExists(string accountNumber);
        Task<int> CountSavingAccountsByUserIds(HashSet<string> userIds);

        Task<Account?>  GetAccountByNumber(string accountNumber);
        Task<List<Account>> GetAllListByIdAsync(string IdCliente);
        Task<Account?> GetAccounByIdAsync(string IdCliente);


        Task<Account?> CreditBalance(string number, decimal amount);
        Task<Account?> DebitBalance(string number, decimal amount);
    }
}
