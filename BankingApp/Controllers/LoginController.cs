using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.User;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAccountServiceForWebAPP _accountServiceForWebApp;
        private readonly UserManager<AppUser> _userManager;

        public LoginController(IAccountServiceForWebAPP accountServiceForWebApp, UserManager<AppUser> userManager)
        {
            _accountServiceForWebApp = accountServiceForWebApp;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            AppUser? userSession = await _userManager.GetUserAsync(User);

            if (userSession != null)
            {
                var user = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");

                if (user != null && user.IsVerified == true)
                {
                    return RedirectToRoute(new { controller = "Home", action = "Index" });
                }
            }

            return View(new LoginViewModel() { Password = "", UserName = "" });
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel vm)
        {
            AppUser? userSession = await _userManager.GetUserAsync(User);

            if (userSession != null)
            {
                var user = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");

                if (user != null && user.IsVerified == true)
                {
                    return RedirectToRoute(new { controller = "Home", action = "Index" });
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Password = "";
                return View(vm);
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(vm.UserName))
            {
                vm.HasError = true;
                vm.Error = "El nombre de usuario no puede estar vacío o contener solo espacios";
                vm.Password = "";
                return View(vm);
            }

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                vm.HasError = true;
                vm.Error = "La contraseña no puede estar vacía o contener solo espacios";
                vm.Password = "";
                return View(vm);
            }

            if (vm.UserName.Length < 3)
            {
                vm.HasError = true;
                vm.Error = "El nombre de usuario debe tener al menos 3 caracteres";
                vm.Password = "";
                return View(vm);
            }

            if (vm.Password.Length < 6)
            {
                vm.HasError = true;
                vm.Error = "La contraseña debe tener al menos 6 caracteres";
                vm.Password = "";
                return View(vm);
            }

            LoginResponseDto userDto = await _accountServiceForWebApp.AuthenticateAsync(new LoginDto()
            {
                Password = vm.Password,
                Username = vm.UserName
            });

            if (userDto != null && !userDto.HasError)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }
            else
            {
                vm.HasError = true;
                vm.Error = userDto?.Error ?? "Error al iniciar sesión";
                vm.Password = "";
                return View(vm);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _accountServiceForWebApp.SignOutAsync();
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public IActionResult Register()
        {
            return View(new RegisterViewModel()
            {
                ConfirmPassword = "",
                Email = "",
                LastName = "",
                Name = "",
                Password = "",
                UserName = "",
                DocumentIdNumber = ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Password = "";
                vm.ConfirmPassword = "";
                return View(vm);
            }

            SaveUserDto dto = new SaveUserDto
            {
                Name = vm.Name,
                LastName = vm.LastName,
                DocumentIdNumber = vm.DocumentIdNumber,
                Email = vm.Email,
                UserName = vm.UserName,
                Password = vm.Password,
                Role = "CLIENT" // Rol por defecto para nuevos usuarios
            };

            string origin = $"{Request.Scheme}://{Request.Host}";

            RegisterUserResponseDto returnUser = await _accountServiceForWebApp.RegisterUser(dto, origin);

            if (returnUser.HasError)
            {
                vm.HasError = true;
                vm.Error = string.Join(", ", returnUser.Errors ?? new List<string>());
                vm.Password = "";
                vm.ConfirmPassword = "";
                return View(vm);
            }

            TempData["Success"] = $"¡Cuenta creada exitosamente! Hemos enviado un correo de confirmación a {vm.Email}. Por favor revisa tu bandeja de entrada y también la carpeta de SPAM.";
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            UserResponseDto response = await _accountServiceForWebApp.ConfirmAccountAsync(userId, token);
            return View("ConfirmEmail", response.Message);
        }

        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel() { UserName = "" });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            string origin = $"{Request.Scheme}://{Request.Host}";

            ForgotPasswordRequestDto dto = new() { Username = vm.UserName, Origin = origin };

            UserResponseDto returnUser = await _accountServiceForWebApp.ForgotPasswordAsync(dto);

            if (returnUser.HasError)
            {
                vm.HasError = true;
                vm.Error = string.Join(", ", returnUser.Errors ?? new List<string>());
                return View(vm);
            }

            TempData["Success"] = "Se ha enviado un correo con las instrucciones para restablecer tu contraseña.";
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            return View(new ResetPasswordViewModel() { UserId = userId, Token = token, Password = "", ConfirmPassword = "" });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Password = "";
                vm.ConfirmPassword = "";
                return View(vm);
            }

            ResetPasswordRequestDto dto = new() { Id = vm.UserId, Password = vm.Password, Token = vm.Token };

            UserResponseDto returnUser = await _accountServiceForWebApp.ResetPasswordAsync(dto);

            if (returnUser.HasError)
        {
                vm.HasError = true;
                vm.Error = string.Join(", ", returnUser.Errors ?? new List<string>());
                vm.Password = "";
                vm.ConfirmPassword = "";
                return View(vm);
            }

            TempData["Success"] = "Tu contraseña ha sido restablecida exitosamente.";
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }



    }
}
