using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public interface ITransacctionRepository : IGenericRepository<Transaction>
    {
        Task<bool> ApproveTransaction(int id, Transaction transaction);
        Task<bool> DeclieneTransaction(int id, Transaction transaction);
        Task<bool> MarkAsCredit(int id, Transaction transaction);
        Task<bool> MarkAsDebit(int id, Transaction transaction);


        Task<List<Transaction>> GetListTransaction(string number);

        Task<List<Transaction>> GetTransactionsByTellerAndDateAsync(string tellerId, DateTime startDate, DateTime endDate);
        Task<List<Transaction>> GetListTransactionByNumberCreditCard(string number);

    }
}