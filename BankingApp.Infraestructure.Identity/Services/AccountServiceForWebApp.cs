using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.User;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;


namespace BankingApp.Infraestructure.Identity.Services
{
    public class AccountServiceForWebAPP : IAccountServiceForWebAPP
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountServiceForWebAPP(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            _signInManager = signInManager;
            _emailService = emailService;
            _userManager = userManager;
        }


        public async Task LogoutAsync()
        {
           await _signInManager.SignOutAsync();
           
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {

            LoginResponseDto responseDto = new LoginResponseDto() { Email = "", Id = "", UserName = "", HasError = false };
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                responseDto.HasError = true;
                responseDto.Error = $"No hay ningun usuario el nombre de usuario {loginDto.Username}";
                return responseDto;
            }
            if (!user.EmailConfirmed)
            {
                responseDto.HasError = true;
                responseDto.Error = $"Esta cuenta no esta activa. Actívala mediante un link que ha sido enviado a tu correo";
                return responseDto;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                responseDto.HasError = true;
                responseDto.Error = $"Usuario o contraseña incorrectos";
                return responseDto;

            }
            var rolesList = await _userManager.GetRolesAsync(user);
            responseDto.Id = user.Id;
            responseDto.UserName = user.UserName ?? "";
            responseDto.Email = user.Email ?? "";
            responseDto.IsVerified = user.EmailConfirmed;
            responseDto.Roles = rolesList.ToList();


            return responseDto;
        }
        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return "No hay ningun usuario";

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return "Cuenta confirmada";
            }

            return "Ocurrio un error mientras se confirmaba la cuenta";
        }
        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<RegisterUserResponseDto> RegisterUser(SaveUserDto saveUserDto, string origin)
        {
            RegisterUserResponseDto response = new() { HasError = false, Email = "", UserName = "" , Id="",LastName="", Name="",PhoneNumber="",ProfileImageUrl="",Role=""};
            var userWithSameUsername = await _userManager.FindByNameAsync(saveUserDto.UserName);
            if (userWithSameUsername != null)
            {
                response.HasError = true;
                response.Error = $"Este nombre de usuario ya esta tomado";
                return response;
            }
            var userWithSameEmail = await _userManager.FindByEmailAsync(saveUserDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Error = $"Este correo ya esta asociado a otra cuenta";
                return response;
            }
            AppUser user = new AppUser()
            {
                Name = saveUserDto.UserName,
                ProfileImageUrl=saveUserDto.ProfileImageUrl,
                LastName= saveUserDto.LastName,
                Email = saveUserDto.Email,
                UserName = saveUserDto.UserName,
                EmailConfirmed = false,
                PhoneNumber=saveUserDto.PhoneNumber,

            };
            var result = await _userManager.CreateAsync(user, saveUserDto.Password);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Ocurrió un error.";


                return response;

            }

            else
            {
                await _userManager.AddToRoleAsync(user, saveUserDto.Role);
                string verificationUri = await GetVerificationEmailUri(user, origin);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = saveUserDto.Email,
                    BodyHtml = $"Por favor, confirma tu cuenta visitando esta URL {verificationUri}",
                    Subject = "Confirma tu registro"
                });


                var rolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.UserName = user.UserName;
                response.Email = user.Email;
                response.IsVerified = user.EmailConfirmed;
                response.PhoneNumber = user.PhoneNumber?? "";
                response.LastName = user.LastName;
                response.Name= user.Name;
                response.ProfileImageUrl = user.ProfileImageUrl;
                response.Role = rolesList.FirstOrDefault() ??"";

                return response;
            }


        }
        public async Task<EditUserResponseDto> SetProfileImage(string   Id, string ProfileImageUrl)
        {
            EditUserResponseDto response = new() { HasError = false, Id = "", Email = "", UserName = "", LastName = "", Name ="",PhoneNumber="", ProfileImageUrl=""};


            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                response.HasError = true;
                response.Error = "No se encontró al usuario";
                return response;
            }
            user.ProfileImageUrl = ProfileImageUrl;
            await _userManager.UpdateAsync(user);

            response.Name = user.Name;
            response.Email = user.Email!;
            response.Name = user.Name;
            response.LastName = user.LastName;
            response.PhoneNumber = user.PhoneNumber!;
            response.ProfileImageUrl = user.ProfileImageUrl;
            response.IsVerified = false;


            return response;
        }

        public async Task<EditUserResponseDto> EditUser(EditUserDto saveUserDto)
        {
            EditUserResponseDto response = new() { HasError = false, Id = "", Email = "", UserName = "" ,LastName="", Name="", PhoneNumber="", ProfileImageUrl=""};
            var userWithSameUsername = await _userManager.FindByIdAsync(saveUserDto.Id);
            if (userWithSameUsername != null && userWithSameUsername.Id!=saveUserDto.Id)
            {
                response.HasError = true;
                response.Error = $"Este nombre de usuario ya esta tomado";
                return response;
            }
         
            var user = await _userManager.FindByIdAsync(saveUserDto.Id??"");
            if (user == null)
            {
                response.HasError = true;
                response.Error = $"El usuario no existe";
                return response;
            }
            user.Id = saveUserDto.Id??"";
            user.Name = saveUserDto.Name;
            user.LastName = saveUserDto.LastName;
            user.PhoneNumber = saveUserDto.PhoneNumber;
            user.ProfileImageUrl = saveUserDto.ProfileImageUrl?? user.ProfileImageUrl;


            var result = await _userManager.UpdateAsync(user);
            if (saveUserDto.Password != null)
            {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user,saveUserDto.Password );
            }
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Ocurrió un error.";
                return response;

            }

            else
            {
               

                if (!string.IsNullOrWhiteSpace(saveUserDto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, saveUserDto.Password);
                }


                response.Id = user.Id;
                response.UserName = user.UserName ??"";
                response.Email = user.Email ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.PhoneNumber = user.PhoneNumber ?? "";
                response.ProfileImageUrl = user.ProfileImageUrl ?? "";



                return response;
            }




        }

        public async Task<UserResponseDto> DeleteUser(string id)
        {
            UserResponseDto response = new UserResponseDto() { HasError = false };
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {

                await _userManager.DeleteAsync(user);
                return response;
            }
            else
            {
                response.HasError = true;
                response.Error = "No se encontro el usuario";
                return response;
            }
        }


        public async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            UserResponseDto response = new UserResponseDto { HasError = false };
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                response.HasError = true;
                response.Error = "No hay ninguna cuenta asociada a este nombre de usuario";
                return response;
            }
            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);

            var resetUri = await GetResetPasswordUri(user, request.Origin);
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = user.Email,
                BodyHtml = $"Por favor, resetea tu contraseña visitando esta URL {resetUri}",
                Subject = "Reset password"
            });
            return response;
        }


        public async Task<EmailConfirmResponseDto> ConfirmEmail (EmailConfirmRequestDto request)
        {
            EmailConfirmResponseDto response = new EmailConfirmResponseDto { HasError = false, Error="" };
            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Error = "No hay ninguna cuenta asociada a este  usuario";
                return response;
            }
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = "El token no es válido";
                return response;
            }
            return response;
        }



       
        public async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new UserResponseDto { HasError = false };
            var user = await _userManager.FindByIdAsync(request.Id);
           
            if (user == null)
            {
                response.HasError = true;
                response.Error = "No hay ninguna cuenta asociada a este  usuario";
                return response;
            }
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = "Ocurrió un error reseteando la contraseña";
                return response;
            }
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return response;
        }

        #region private methods

        private async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Accounts/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));

            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);
            return verificationUri;

        }
        private async Task<string> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Accounts/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));

            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;

        }
        #endregion
    }
}
