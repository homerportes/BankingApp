using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.User;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IAccountServiceForWebAPP
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task<EmailConfirmResponseDto> ConfirmEmail(EmailConfirmRequestDto request);
        Task<UserResponseDto> DeleteUser(string id);
        Task<EditUserResponseDto> EditUser(EditUserDto editUserDto);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task LogoutAsync();
        Task<RegisterUserResponseDto> RegisterUser(SaveUserDto saveUserDto, string origin);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<EditUserResponseDto> SetProfileImage(string Id, string ProfileImageUrl);
        Task SignOutAsync();
    }
}