using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Transaction.Transference;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.Transferences;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Core.Application.Services
{
    public abstract class BaseSavingAccountService :GenericService<Account,AccountDto>, IBaseSavingAccountService
    {
        private readonly IMapper _mapper;
        private readonly ITransacctionRepository _transactionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountRepository _repo;

        public BaseSavingAccountService(IAccountRepository repo, IMapper mapper, ITransacctionRepository transacctionRepository, IUnitOfWork unitOfWork) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _transactionRepo= transacctionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AccountDto?> GetAccountByClientId(string clientId)
        {
            var entity = await _repo.GetAllQuery().Where(r => r.UserId == clientId).FirstOrDefaultAsync();
            if (entity == null) return default;
            return _mapper.Map<AccountDto>(entity);
        }
        public async Task<string> GenerateAccountNumber()
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

        public async Task<List<string>?> GetActiveAccountsByClientId(string clientId)
        {
          return  await _repo.GetAllQuery().Where(r => r.UserId == clientId &&r.Status==AccountStatus.ACTIVE).Select(r => r.Number).ToListAsync();
        }

        public async Task<bool> AccountHasEnoughFounds(string accountNumber, decimal requestAmount)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || requestAmount <= 0)
                return false;

            var account = await _repo.GetAllQuery()
                                     .FirstOrDefaultAsync(r => r.Number == accountNumber);

            if (account == null)
                return false;

            return account.Balance >= requestAmount;
        }

        public async Task<TransferenceResponseDto> ExecuteTransference(TransferenceRequestDto tranferenceRequest)
        {
            var response = new TransferenceResponseDto
            {
                IsSuccessful = true,
                Message = "Transacción realizada correctamente"
            };

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Obtener cuentas de origen y destino
                var accountFrom = await _repo.GetAllQuery()
                                             .FirstAsync(a => a.Number == tranferenceRequest.AccountNumberFrom);
                var accountTo = await _repo.GetAllQuery()
                                           .FirstAsync(a => a.Number == tranferenceRequest.AccountNumberTo);

                

                if (accountFrom.Balance < tranferenceRequest.Amount)
                {
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen.");
                }

                accountFrom.Balance -= tranferenceRequest.Amount;
                accountTo.Balance += tranferenceRequest.Amount;

                await _repo.UpdateAsync(accountFrom.Id, accountFrom);
                await _repo.UpdateAsync(accountTo.Id, accountTo);

                var now = DateTime.Now;
                var operationId = _transactionRepo.GenerateOperationId();
                await _transactionRepo.AddAsync(new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountNumber = accountFrom.Number,
                    Beneficiary = accountTo.Number,
                    Type = TransactionType.DEBIT,
                    Origin = accountFrom.Number,
                    Amount = tranferenceRequest.Amount,
                    Status = OperationStatus.APPROVED,
                    OperationId= operationId,
                    DateTime = now
                });

                await _transactionRepo.AddAsync(new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountNumber = accountTo.Number,
                    Beneficiary = accountTo.Number,
                    Type = TransactionType.CREDIT,
                    Origin = accountFrom.Number,

                    Amount = tranferenceRequest.Amount,
                    Status = OperationStatus.APPROVED,
                    DateTime = now,
                     OperationId= operationId
                });

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                response.IsSuccessful = false;
                response.Message = $"Ocurrió un error durante la transferencia: {ex.Message}";
            }

            return response;
        }




    }
}
