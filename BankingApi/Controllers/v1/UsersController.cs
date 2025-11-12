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
        private readonly ISavingAccountServiceForApi _bankAccountService;
        private readonly ICommerceService _commerceService;

        public UsersController(IUserService userService, IAccountServiceForWebApi accountService, ISavingAccountServiceForApi bankAccountService, ICommerceService commerceService)
        {
            _userService = userService;
            _accountService = accountService;
            _bankAccountService = bankAccountService;
            _commerceService = commerceService;
        }

        [HttpGet(Name = "ObtenerTodosLosUsuarios")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, int pageSize = 20, string? rol = null)
        {
            try
            {
                var result = await _userService.GetAllExceptCommerce(page, pageSize, rol);
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }




        }

        [HttpGet("commerce", Name = "ObtenerUsuariosComercios")]
        public async Task<IActionResult> GetAllCommerce([FromQuery] int page = 1, int pageSize = 20, string? rol = null)
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
        [HttpPost(Name = "CrearUsuario")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {

                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }
            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.UserName) || dto.UserName == "string") missingFields.Add("usuario");
            if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string") missingFields.Add("correo");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password == "string") missingFields.Add("contrasena");
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword) || dto.ConfirmPassword == "string") missingFields.Add("ConfirmContrasena");
            if (string.IsNullOrWhiteSpace(dto.Role) || dto.Role == "string") missingFields.Add("rol");
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name == "string") missingFields.Add("nombre");
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
            if (!new EmailAddressAttribute().IsValid(dto.Email))
            {
                return BadRequest("Correo con formato incorrecto");

            }
            if (dto.DocumentIdNumber.Length != 11 || !dto.DocumentIdNumber.All(char.IsDigit))
            {
                return BadRequest("La cédula debe contener exactamente 11 dígitos numéricos");

            }
            List<string> allRoles = EnumMapper<AppRoles>.GetAllAliases()
                .Select(a => a.ToLower())
                .Distinct()
                .ToList();

            if (!allRoles.Contains(dto.Role.ToLower()))
            {
                return BadRequest("Rol inválido");

            }

            try



            {


                var enumoption = EnumMapper<AppRoles>.FromString(dto.Role);


                var result = await _accountService.RegisterUser(new SaveUserDto
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Password = dto.Password,
                    DocumentIdNumber = dto.DocumentIdNumber,
                    Roles = new List<string> { enumoption.ToString() },
                }, null, true);



                if (result == null)
                {
                    return BadRequest(result?.Errors);
                }

                if (result.HasError)
                {
                    return Conflict("Usuario o correo ya registrado");
                }

                if (enumoption == AppRoles.CLIENT)
                {


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


        [HttpPost("commerce/{commerceId}", Name = "CrearUsuarioComercio")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterCommerce([FromRoute] int? commerceId, [FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.UserName) || dto.UserName == "string") missingFields.Add("usuario");
            if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string") missingFields.Add("correo");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password == "string") missingFields.Add("contrasena");
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword) || dto.ConfirmPassword == "string") missingFields.Add("ConfirmContrasena");
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name == "string") missingFields.Add("nombre");
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
                return BadRequest("Las contraseñas no coinciden");
            }

            if (!new EmailAddressAttribute().IsValid(dto.Email))
            {
                return BadRequest("Correo con formato incorrecto");
            }

            if (dto.DocumentIdNumber.Length != 11 || !dto.DocumentIdNumber.All(char.IsDigit))
            {
                return BadRequest("La cédula debe contener exactamente 11 dígitos numéricos");
            }

            if (commerceId == null)
            {
                return BadRequest("El ID del comercio es requerido");
            }

            List<string> allRoles = EnumMapper<AppRoles>.GetAllAliases()
            .Select(a => a.ToLower())
            .Distinct()
            .ToList();

            if (!allRoles.Contains(dto.Role.ToLower()))
            {
                return BadRequest("Rol inválido");

            }
            try
            {


                var commerce = await _commerceService.GetByIdAsync(commerceId ?? 0);
                if (commerce == null)
                {
                    return BadRequest("El comercio especificado no existe");
                }

                if (commerce.UserId != null) return BadRequest("El comercio ya tiene un usuario asociado");



                var enumoption = EnumMapper<AppRoles>.FromString(dto.Role);

                var result = await _accountService.RegisterUser(new SaveUserDto
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Password = dto.Password,
                    DocumentIdNumber = dto.DocumentIdNumber,
                    Roles = new List<string> { enumoption.ToString() },
                }, null, true);

                if (result == null)
                {
                    return BadRequest(result?.Errors);
                }

                if (result.HasError)
                {
                    return Conflict("Usuario o correo ya registrado");
                }


                if (enumoption == AppRoles.CLIENT)
                {
                    await _commerceService.SetUser(commerceId ?? 0, result.Id);

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

        [HttpPut("{id}", Name = "ActualizarUsuario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("El ID del usuario es requerido");
            }

            try
            {

                var missingFields = new List<string>();

                if (string.IsNullOrWhiteSpace(dto.UserName) || dto.UserName == "string") missingFields.Add("usuario");
                if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string") missingFields.Add("correo");
                if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name == "string") missingFields.Add("nombre");
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

                if (!new EmailAddressAttribute().IsValid(dto.Email))
                {
                    return BadRequest("Correo con formato incorrecto");
                }

                if (dto.DocumentIdNumber.Length != 11 || !dto.DocumentIdNumber.All(char.IsDigit))
                {
                    return BadRequest("La cédula debe contener exactamente 11 dígitos numéricos");
                }

                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    if (string.IsNullOrWhiteSpace(dto.ConfirmPassword) || dto.Password != dto.ConfirmPassword)
                    {
                        return BadRequest("Las contraseñas no coinciden");
                    }
                }



                var existingUser = await _accountService.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound("El usuario especificado no existe");
                }


                var result = await _accountService.EditUser(new SaveUserDto
                {
                    Id = id,
                    UserName = dto.UserName,
                    Email = dto.Email,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Password = dto.Password ?? string.Empty,
                    DocumentIdNumber = dto.DocumentIdNumber
                }, null, false, true);

                if (result == null || result.HasError)
                {
                    return Conflict(new
                    {
                        mensaje = "Error al actualizar el usuario",
                        errores = result?.Errors
                    });
                }

                var SavingAccount = await _bankAccountService.GetAccountByClientId(result.Id);

                if (SavingAccount != null)
                {
                    SavingAccount.Balance += dto.AditionalBalance ?? 0;
                    await _bankAccountService.UpdateAsync(SavingAccount.Id, SavingAccount);

                }


                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id}/status", Name = "CambiarEstadoUsuario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatus([FromRoute] string id, [FromBody] bool? status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("El ID del usuario es requerido");
            }


            if (status == null)
            {
                return BadRequest("Estructura inválida");

            }
            try
            {
                var updateResult = await _accountService.UpdateUserStatusAsync(id, status ?? false);

                if (updateResult.HasError)
                {
                    if (updateResult.Errors!.Any())
                    {
                        return NotFound("El usuario especificado no existe");
                    }

                    return BadRequest(new
                    {
                        mensaje = "Error al actualizar el estado del usuario",
                        errores = updateResult.Errors
                    });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "ObtenerUsuarioPorId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("El ID del usuario es requerido");
            }

            try
            {
                var user = await _accountService.GetUserById(id);
                if (user == null)
                {
                    return NotFound("El usuario especificado no existe");
                }

                // Obtener cuenta principal si el usuario es cliente
                PrimaryAccountDto? primaryAccount = null;
                if (user.Role.ToUpper() == "CLIENT" || user.Role.ToUpper() == "CLIENTE")
                {
                    var account = await _bankAccountService.GetAccountByClientId(user.Id);
                    if (account != null && account.Type == AccountType.PRIMARY)
                    {
                        primaryAccount = new PrimaryAccountDto
                        {
                            AccountNumber = account.Number,
                            Balance = account.Balance
                        };
                    }
                }

                var userDetail = new UserDetailDto
                {
                    UserName = user.UserName,
                    Name = user.Name,
                    LastName = user.LastName,
                    DocumentIdNumber = user.DocumentIdNumber,
                    Email = user.Email,
                    Role = user.Role,
                    Status = user.Status,
                    PrimaryAccount = primaryAccount
                };

                return Ok(JsonConvert.SerializeObject(userDetail));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
