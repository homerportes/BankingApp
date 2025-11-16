using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Transaction.Transference;
using BankingApp.Core.Application.Dtos.User;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IUserAccountManagementService
    {
        Task<bool> AccountHasEnoughFounds(string accountNumber, decimal requestAmount);
        public Task ChangeBalanceForClient(string id, decimal AdditionalBalance);
        Task<RegisterUserWithAccountResponseDto> CreateUserWithAmount(CreateUserDto request, string AdminId, bool ForApi= false,string ? origin = null);
        Task<RegisterUserWithAccountResponseDto> EditUserAndAmountAsync(UpdateUserDto request, string AdminId,bool ForApi= false, string? origin = null);
        Task<List<string>> GetCurrentUserActiveAccounts(string currentUserName);
        Task<AccountDto?> GetMainSavingAccount(string clientId);
        Task<TransferenceResponseDto> TransferAmountToAccount(TransferenceRequestDto tranferenceRequest);
    }
}
