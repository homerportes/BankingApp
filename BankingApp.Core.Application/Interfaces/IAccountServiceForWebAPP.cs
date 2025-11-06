using BankingApp.Core.Application.Dtos.Login;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IAccountServiceForWebAPP: IBaseAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task SignOutAsync();
    }
}