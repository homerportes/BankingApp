
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Transaction;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IHomeClietService
    {


        Task<List<DataAccountaHomeClientDto>> GetDataAccountClient(string idUser);
        Task<List<DataTransactionHomeClientDto>> GetDataListTransaction(string number);
        Task<List<DataLoanHomeClientDto>> GetDataLoanHomeClient(string ClientId);
        Task<List<DetailsLoanHomeClientDto>> GetDetailsLoanHomeClient(Guid number);
        Task<List<DataHomeClientCreditCardDto>> GetDetaCreditCardHomeClient(string idUsuario);
        Task<List<DetailsCreditCardHomeClientDto>> GetDetailsCreditCardHomeClient(string number);

    }
}
