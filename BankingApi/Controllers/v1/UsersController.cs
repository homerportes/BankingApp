using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;


namespace BankingApi.Controllers.v1
{

    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]
    [Route("api/v{version::apiVersion}/users")]

    public class UsersController : BaseApiController
    {
        private IUserService _userService;
        private readonly IAccountServiceForWebApi _accountService;
        private readonly IBankAccountService _bankAccountService;

        public UsersController(IUserService userService, IAccountServiceForWebApi accountService, IBankAccountService bankAccountService)
        {
            _userService = userService;
            _accountService = accountService;
            _bankAccountService = bankAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, int pageSize = 20, string? rol = null)
        {
            try
            {
                var result = await _userService.GetAllExceptCommerce(page, pageSize, rol);
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }




        }

        [HttpGet("commerce")]
        public async Task<IActionResult> GetAllCommerce([FromBody] int page = 1, int pageSize = 20, string? rol = null)
        {
            try
            {
                var result = await _userService.GetAllOnlyCommerce(page, pageSize, rol);
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }




        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {

                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }
            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.UserName) || dto.UserName=="string") missingFields.Add("usuario");
            if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string") missingFields.Add("correo");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password == "string") missingFields.Add("contrasena");
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword) || dto.ConfirmPassword == "string") missingFields.Add("ConfirmContrasena");
            if (string.IsNullOrWhiteSpace(dto.Role )|| dto.Role=="string") missingFields.Add("rol");
            if (string.IsNullOrWhiteSpace(dto.Name)|| dto.Name == "string") missingFields.Add("nombre");
            if (string.IsNullOrWhiteSpace(dto.LastName) || dto.LastName == "string") missingFields.Add("apellido");
            if (string.IsNullOrWhiteSpace(dto.DocumentIdNumber) || dto.DocumentIdNumber == "string") missingFields.Add("cedula");

            if (missingFields.Any())
            {
                return BadRequest(new
                {
                    mensaje = "Faltan uno o más campos requeridos.",
                    camposFaltantes = missingFields
                });
            }

            if (dto.ConfirmPassword != dto.Password)
            {
                return BadRequest("La contraseñas no coinciden");

            }
            if (!new EmailAddressAttribute().IsValid( dto.Email))
            {
                return BadRequest("Correo con formato incorrecto");

            }
            if (dto.DocumentIdNumber.Length<11 || !dto.DocumentIdNumber.All(char.IsDigit))
            {
                return BadRequest("La cedula debe contener 11 caracteres numéricos");

            }
            List<string> allRoles = new List<string>();
            var roles = Enum.GetNames(typeof(AppRoles)).ToList();
              foreach (var role in roles)
            {
                allRoles.Add(role.ToLower());
                allRoles.Add(RoleTranslator.Translate(role).ToLower());
            }
            if (!allRoles.Contains(dto.Role.ToLower()))
            {
                return BadRequest("Rol inválido");

            }

            try



            {
                List<string> Roles = new List<string>();
                if (dto.Role.ToLower() == "cliente" || dto.Role.ToLower() == AppRoles.CLIENT.ToString().ToLower())
                    Roles.Add(AppRoles.CLIENT.ToString());

                if (dto.Role.ToLower() == "cajero" || dto.Role.ToLower() == AppRoles.TELLER.ToString().ToLower())
                    Roles.Add(AppRoles.TELLER.ToString());
                if (dto.Role.ToLower() == "admin" ||dto.Role.ToLower()=="administrador"|| dto.Role.ToLower() == AppRoles.ADMIN.ToString().ToLower())
                    Roles.Add(AppRoles.ADMIN.ToString());



                var result = await _accountService.RegisterUser(new SaveUserDto
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Password = dto.Password,
                    DocumentIdNumber = dto.DocumentIdNumber,
                    Roles = Roles,
                }, null, true);

                

                if (result == null)
                {
                    return BadRequest(result?.Errors);
                }

                if (result.HasError)
                {
                    return Conflict("Usuario o correo ya registrado");
                }

                if (dto.Role.ToLower()=="client" || dto.Role.ToLower() == "cliente"){

                    var accountNumber = await _bankAccountService.GenerateAccountNumber();
                    await _bankAccountService.AddAsync(new AccountDto
                    {
                        Id = 0,
                        ClientId = result.Id,
                        
                        Number = accountNumber,
                        Type = AccountType.PRIMARY,
                        Balance = dto.InitialAmount ?? 0
                    });
                }

                return Created();
               
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }


    }
}
