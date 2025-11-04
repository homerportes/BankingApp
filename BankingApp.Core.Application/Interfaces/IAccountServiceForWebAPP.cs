using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.User;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IAccountServiceForWebAPP: IBaseAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
   
        
        Task SignOutAsync();
    }
}