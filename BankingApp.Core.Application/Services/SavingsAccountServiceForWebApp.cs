using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.SavingsAccount;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Core.Application.Services
{
    public class SavingsAccountServiceForWebApp : BaseSavingAccountService, ISavingsAccountServiceForWebApp
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ITransacctionRepository transacctionRepository;
        public SavingsAccountServiceForWebApp(
            IAccountRepository accountRepository,
            IUserService userService,
            IMapper mapper, IUnitOfWork unitOfWork, ITransacctionRepository transacctionRepository) : base(accountRepository, mapper, transacctionRepository, unitOfWork)
        {
            _accountRepository = accountRepository;
            _userService = userService;
            _mapper = mapper;
            this.transacctionRepository = transacctionRepository;
        }



        public async Task<List<AccountDto>> GetAllAccountsAsync(int page, int pageSize, string? cedula = null, string? estado = null, string? tipo = null)
        {
            var accounts = await _accountRepository.GetAllList();
            var accountsList = accounts?.ToList() ?? new List<Account>();

            var activeUserIds = await _userService.GetActiveUserIdsAsync();

            accountsList = accountsList.Where(a => activeUserIds.Contains(a.UserId)).ToList();

            if (!string.IsNullOrEmpty(cedula))
            {
                cedula = cedula.Trim().Replace("-", "");
                var user = await _userService.GetByDocumentId(cedula);
                if (user != null)
                {
                    accountsList = accountsList.Where(a => a.UserId == user.Id).ToList();
                }
                else
                {
                    return new List<AccountDto>();
                }
            }



            if (estado == null && cedula != null)
            {

                accountsList = accountsList.Where(a => a.Status == AccountStatus.ACTIVE || a.Status == AccountStatus.CANCELLED)
                  .ToList();

            }
            else if (!string.IsNullOrEmpty(estado))
            {
                if (Enum.TryParse<AccountStatus>(estado, out var statusEnum))
                {
                    accountsList = accountsList.Where(a => a.Status == statusEnum).ToList();
                }
            }
            else
            {

                accountsList = accountsList.Where(a => a.Status == AccountStatus.ACTIVE).ToList();
            }



            if (!string.IsNullOrEmpty(tipo))
            {
                if (Enum.TryParse<AccountType>(tipo, out var typeEnum))
                {
                    accountsList = accountsList.Where(a => a.Type == typeEnum).ToList();
                }
            }


            var sortedAccounts = accountsList
                .OrderByDescending(a => a.Status == AccountStatus.ACTIVE) 
                .ThenByDescending(a => a.CreatedAt)
                .ToList();

            return _mapper.Map<List<AccountDto>>(sortedAccounts);
        }

        public async Task<AccountDto?> GetAccountByIdAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account != null ? _mapper.Map<AccountDto>(account) : null;
        }

        public async Task<AccountDto?> GetAccountByNumberAsync(string accountNumber)
        {
            var accounts = await _accountRepository.GetAllList();
            var account = accounts?.FirstOrDefault(a => a.Number == accountNumber);
            return account != null ? _mapper.Map<AccountDto>(account) : null;
        }

        public async Task<List<AccountDto>> GetAccountsByClientIdAsync(string clientId)
        {
            var accounts = await _accountRepository.GetAllList();
            var clientAccounts = accounts?.Where(a => a.UserId == clientId).ToList() ?? new List<Account>();
            return _mapper.Map<List<AccountDto>>(clientAccounts);
        }

        public async Task<AccountDto?> GetPrimaryAccountByClientIdAsync(string clientId)
        {
            var accounts = await _accountRepository.GetAllList();
            var primaryAccount = accounts?.FirstOrDefault(a =>
                a.UserId == clientId &&
                a.Type == AccountType.PRIMARY &&
                a.Status == AccountStatus.ACTIVE);

            return primaryAccount != null ? _mapper.Map<AccountDto>(primaryAccount) : null;
        }

        public async Task<AccountDto> CreateSecondaryAccountAsync(AccountDto accountDto, string adminId)
        {
            var accountNumber = await GenerateAccountNumber();


            var account = _mapper.Map<Account>(accountDto);
            account.Number = accountNumber;
            account.Type = AccountType.SECONDARY;
            account.Status = AccountStatus.ACTIVE;
            account.CreatedAt = DateTime.Now;
            account.AdminId = adminId;
            account.Balance = accountDto.Balance;
            account.UserId = accountDto.UserId;
            var createdAccount = await _accountRepository.AddAsync(account);



            var operationId1 = transacctionRepository.GenerateOperationId();
            var transaction = new Transaction
            {
                Amount = accountDto.Balance,
                DateTime = DateTime.Now,
                Type = TransactionType.CREDIT,
                Origin = "System***",
                Beneficiary = accountNumber,
                AccountNumber = accountNumber,
                AccountId = account.Id,
                Status = OperationStatus.APPROVED,
                Description = DescriptionTransaction.DEPOSIT,
                OperationId = operationId1,
                TellerId = null
            };

            await transacctionRepository.AddAsync(transaction);

            return _mapper.Map<AccountDto>(createdAccount);
        }



        public async Task<bool> CancelAccountAsync(int accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);

            if (account == null || account.Type == AccountType.PRIMARY)
                return false;

            if (account.Balance > 0)
            {
                var primaryAccount = await GetPrimaryAccountByClientIdAsync(account.UserId);
                if (primaryAccount != null)
                {
                    var primaryAccountEntity = await _accountRepository.GetByIdAsync(primaryAccount.Id);
                    if (primaryAccountEntity != null)
                    {
                        primaryAccountEntity.Balance += account.Balance;
                        await _accountRepository.UpdateAsync(primaryAccount.Id, primaryAccountEntity);


                        var operationId1 = transacctionRepository.GenerateOperationId();
                        var transaction = new Transaction
                        {
                            Amount = account.Balance,
                            DateTime = DateTime.Now,
                            Type = TransactionType.CREDIT,
                            Origin = account.Number,
                            Beneficiary = primaryAccountEntity.Number,
                            AccountNumber = primaryAccountEntity.Number,
                            AccountId = account.Id,
                            Status = OperationStatus.APPROVED,
                            Description = DescriptionTransaction.TRANSFER,
                            OperationId = operationId1,
                            TellerId = null
                        };

                        await transacctionRepository.AddAsync(transaction);

                    }
                    account.Balance = 0;
                }
            }


            account.Status = AccountStatus.CANCELLED;
            await _accountRepository.UpdateAsync(accountId, account);

            return true;
        }

        public async Task<int> GetTotalAccountsCountAsync(string? cedula = null, string? estado = null, string? tipo = null)
        {
            var accounts = await GetAllAccountsAsync(1, int.MaxValue, cedula, estado, tipo);
            return accounts.Count;
        }

        public async Task<List<TransactionViewModel>> GetAccountTransactionsAsync(int accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);

            if (account == null)
                return new List<TransactionViewModel>();

            var accounts = await _accountRepository.GetAllListWithInclude(new List<string> { "Transactions" });
            var accountWithTransactions = accounts?.FirstOrDefault(a => a.Id == accountId);

            if (accountWithTransactions?.Transactions == null || !accountWithTransactions.Transactions.Any())
                return new List<TransactionViewModel>();

            var transactions = _mapper.Map<List<TransactionViewModel>>(accountWithTransactions.Transactions);
            return transactions.OrderByDescending(t => t.TransactionDate).ToList();
        }


    }
}
