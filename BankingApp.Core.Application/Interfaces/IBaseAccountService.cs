using BankingApp.Core.Application.Dtos.User;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IBaseAccountService
    {
        Task<UserResponseDto> ConfirmAccountAsync(string token, string? userId = null, bool isForApi = false);
        Task<UserResponseDto> DeleteAsync(string id);
        Task<EditUserResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false);
        Task<List<UserDto>> GetAllUser(bool? isActive = true);
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserById(string Id);
        Task<UserDto?> GetUserByUserName(string userName);
        Task<RegisterUserResponseDto> RegisterUser(SaveUserDto saveDto, string? origin, bool? isApi = false);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<EditUserResponseDto> UpdateUserStatusAsync(string userId, bool isActive);
    }
}