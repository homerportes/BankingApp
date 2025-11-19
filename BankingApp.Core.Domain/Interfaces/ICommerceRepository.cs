using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface ICommerceRepository : IGenericRepository<Commerce>
    {
        Task<bool> CommerceIsValid(int id);
        Task<string?> GetAssociatedCommerceUserId(int commerceId);
        Task<List<string>> GetAssociatesCommerceUsersId(int commerceId);
    }
}
