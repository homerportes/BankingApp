using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Services
{
    public class BankAccountService : GenericService<Account, AccountDto> , IBankAccountService
    {

        private readonly IAccountRepository _repo;
        private readonly IMapper _mapper;
        public BankAccountService(IAccountRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task <AccountDto?> GetAccountByClientId(string clientId)
        {
           var entity= await _repo.GetAllQuery().Where(r => r.ClientId == clientId).FirstOrDefaultAsync();
            if (entity == null) return default;
            return _mapper.Map<AccountDto>(entity);
        }

        public async Task<string> GenerateAccountNumber ()
        {
            bool accountExists = false;
            string accountNumber;
            do
            {
                accountNumber = new string(
                         Guid.NewGuid()
                         .ToString("N")
                         .Where(char.IsDigit)
                         .Take(9)
                         .ToArray());
                accountExists = await _repo.AccountExists(accountNumber);

            } while (accountExists);
          
            return accountNumber;
        }
    }
}
