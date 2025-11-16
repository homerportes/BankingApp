
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Transaction;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IHomeClietService
    {


        Task<List<DataAccountaHomeClientDto>> GetDataAccountClient(string idUser);
        Task<List<DataTransactionHomeClientDto>> GetDataListTransaction(string number);

    }
}
