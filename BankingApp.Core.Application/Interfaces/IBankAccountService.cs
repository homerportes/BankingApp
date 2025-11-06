using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IBankAccountService : IGenericService<Account, AccountDto>
    {
        Task<string> GenerateAccountNumber();
    }
}
