

using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Mappings.EntitiesAndDtos;
using System.Transactions;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ICashAdvancesServices : IGenericService<Transaction,CreateTransactionDto>
    {


        Task<ValidateTotalAmountOwedResponseDto> ValidateTotalAmountOwedCreditCard(string number,decimal amount);
        Task<bool> CreditTotalAmountOwedAsync(string number, decimal amount);

    }
}
