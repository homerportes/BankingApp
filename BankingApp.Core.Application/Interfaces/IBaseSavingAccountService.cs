using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Transaction.Transference;
using BankingApp.Core.Application.Services;
using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IBaseSavingAccountService : IGenericService<Account, AccountDto>
    {
        Task<bool> AccountHasEnoughFounds(string accountNumber, decimal requestAmount);
        Task<TransferenceResponseDto> ExecuteTransference(TransferenceRequestDto tranferenceRequest);
        public Task<string> GenerateAccountNumber();
        public Task<AccountDto?> GetAccountByClientId(string clientId);
        Task<List<string>?> GetActiveAccountsByClientId(string clientId);
    }
}
