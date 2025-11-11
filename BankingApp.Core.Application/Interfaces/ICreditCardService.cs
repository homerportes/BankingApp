using BankingApp.Core.Application.Dtos.CreditCard;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ICreditCardService
    {
        Task<List<CreditCardDto>> GetAllAsync(int page, int pageSize, string? status = null);
        Task<List<CreditCardDto>> GetByClientDocumentAsync(string documentId, string? status = null);
        Task<CreditCardDto?> GetByIdAsync(int id);
        Task<CreditCardDto?> GetByNumberAsync(string cardNumber);
        Task<string> GenerateCardNumber();
        Task<string> GenerateCVC();
        Task<CreditCardDto> CreateAsync(string clientId, decimal creditLimit, string adminId);
        Task<bool> UpdateCreditLimitAsync(int id, decimal newLimit);
        Task<bool> CancelCardAsync(int id);
        Task<List<PurchaseDto>> GetPurchasesByCardIdAsync(int cardId);
    }
}
