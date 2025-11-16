
using BankingApp.Core.Application.Dtos.Payment;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResultDto> Pay(PaymentRequestDto requestDto, int commerceId);
    }
}
