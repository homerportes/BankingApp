using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Infraestructure.Persistence.Repositories;

namespace BankingApp.Core.Application.Services
{
    public class TranferencesService : ITranferencesService
    {
        private readonly ITransacctionRepository _transacctionRepository;

        public TranferencesService(ITransacctionRepository transacctionRepository)
        {
            _transacctionRepository = transacctionRepository;
        }

        public async Task<Transaction> CreateDeclinedTransactionAsync(string accountFrom, string accountTo, decimal amount)
        {
            var now = DateTime.Now;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountNumber = accountFrom,
                Beneficiary = accountTo,
                Type = TransactionType.DEBIT,
                Origin = accountFrom,
                Amount = amount,
                Description = DescriptionTransaction.TRANSFER,
                Status = OperationStatus.DECLINED,
                DateTime = now
            };

            await _transacctionRepository.AddAsync(transaction);
            return transaction;
        }
    }
}
