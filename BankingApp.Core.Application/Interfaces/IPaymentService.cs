
using BankingApp.Core.Application.Dtos.Payment;
using BankingApp.Core.Application.Dtos.Transaction;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<List<CommerceTransactionDto>> GetTransactionsForCommerceId(int commerceId, int page, int pageSize);
        Task<PaymentResultDto> Pay(PaymentRequestDto requestDto, int commerceId);
    }
}
