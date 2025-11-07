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

        [HttpGet(Name = "ObtenerTodosLosUsuarios")]
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
                allRoles.Add(EnumTranslator.Translate(role).ToLower());
            }
            if (!allRoles.Contains(dto.Role.ToLower()))
            {
                return BadRequest("Rol inválido");

            }

            try



            {
                List<string> Roles = new List<string>();
                if (dto.Role.ToLower() == "cliente" || dto.Role.ToLower() == AppRoles.CLIENT.ToString().ToLower())
                {
                    Roles.Add(AppRoles.CLIENT.ToString());
                }
                else if (dto.Role.ToLower() == "cajero" || dto.Role.ToLower() == AppRoles.TELLER.ToString().ToLower())
                {
                    Roles.Add(AppRoles.TELLER.ToString());
                }
                else if (dto.Role.ToLower() == "admin" ||dto.Role.ToLower()=="administrador"|| dto.Role.ToLower() == AppRoles.ADMIN.ToString().ToLower())
                {
                    Roles.Add(AppRoles.ADMIN.ToString());
                }

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

        [HttpPost("commerce/{commerceId}", Name = "CrearUsuarioComercio")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterCommerce([FromRoute] string commerceId, [FromBody] CreateUserDto dto)
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

            if (dto.DocumentIdNumber.Length < 11 || !dto.DocumentIdNumber.All(char.IsDigit))
            {
                return BadRequest("La cédula debe contener 11 caracteres numéricos");
            }

            // Verificar que el commerceId sea válido
            if (string.IsNullOrWhiteSpace(commerceId))
            {
                return BadRequest("El ID del comercio es requerido");
            }

            try
            {
                // Verificar que el comercio existe
                var commerce = await _accountService.GetUserById(commerceId);
                if (commerce == null)
                {
                    return NotFound("El comercio especificado no existe");
                }

                // Verificar que el usuario encontrado tiene rol COMMERCE
                if (commerce.Role?.ToUpper() != AppRoles.COMMERCE.ToString())
                {
                    return BadRequest("El ID proporcionado no corresponde a un comercio");
                }

                List<string> Roles = new List<string> { AppRoles.COMMERCE.ToString() };

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

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, new
                {
                    id = result.Id,
                    usuario = result.UserName,
                    nombre = result.Name,
                    apellido = result.LastName,
                    correo = result.Email,
                    comercioId = commerceId
                });
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
                // Verificar que el usuario existe
                var existingUser = await _accountService.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound("El usuario especificado no existe");
                }

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

                if (dto.DocumentIdNumber.Length < 11 || !dto.DocumentIdNumber.All(char.IsDigit))
                {
                    return BadRequest("La cédula debe contener 11 caracteres numéricos");
                }

                // Si se proporciona una nueva contraseña, validarla
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    if (string.IsNullOrWhiteSpace(dto.ConfirmPassword) || dto.Password != dto.ConfirmPassword)
                    {
                        return BadRequest("Las contraseñas no coinciden");
                    }
                }

                List<string> Roles = new List<string>();
                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
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

                    if (dto.Role.ToLower() == "cliente" || dto.Role.ToLower() == AppRoles.CLIENT.ToString().ToLower())
                        Roles.Add(AppRoles.CLIENT.ToString());
                    else if (dto.Role.ToLower() == "cajero" || dto.Role.ToLower() == AppRoles.TELLER.ToString().ToLower())
                        Roles.Add(AppRoles.TELLER.ToString());
                    else if (dto.Role.ToLower() == "admin" || dto.Role.ToLower() == "administrador" || dto.Role.ToLower() == AppRoles.ADMIN.ToString().ToLower())
                        Roles.Add(AppRoles.ADMIN.ToString());
                    else if (dto.Role.ToLower() == "comercio" || dto.Role.ToLower() == AppRoles.COMMERCE.ToString().ToLower())
                        Roles.Add(AppRoles.COMMERCE.ToString());
                }
                else
                {
                    // Si no se proporciona rol, mantener el actual
                    Roles.Add(existingUser.Role ?? AppRoles.CLIENT.ToString());
                }

                var result = await _accountService.EditUser(new SaveUserDto
                {
                    Id = id,
                    UserName = dto.UserName,
                    Email = dto.Email,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Password = dto.Password ?? string.Empty,
                    DocumentIdNumber = dto.DocumentIdNumber,
                    Roles = Roles,
                }, null, false, true);

                if (result == null || result.HasError)
                {
                    return BadRequest(new
                    {
                        mensaje = "Error al actualizar el usuario",
                        errores = result?.Errors
                    });
                }

                return Ok(new
                {
                    id = result.Id,
                    usuario = result.UserName,
                    nombre = result.Name,
                    apellido = result.LastName,
                    correo = result.Email,
                    cedula = existingUser.DocumentIdNumber,
                    verificado = result.IsVerified
                });
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
        public async Task<IActionResult> UpdateStatus([FromRoute] string id, [FromBody] UpdateUserStatusDto dto)
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
                // Verificar que el usuario existe y actualizar su estado
                var updateResult = await _accountService.UpdateUserStatusAsync(id, dto.IsActive);
                
                if (updateResult.HasError)
                {
                    if (updateResult.Errors?.Any(e => e.Contains("no existe")) == true)
                    {
                        return NotFound("El usuario especificado no existe");
                    }
                    
                    return BadRequest(new
                    {
                        mensaje = "Error al actualizar el estado del usuario",
                        errores = updateResult.Errors
                    });
                }

                return Ok(new
                {
                    id = updateResult.Id,
                    usuario = updateResult.UserName,
                    estado = dto.IsActive ? "activo" : "inactivo",
                    verificado = updateResult.IsVerified
                });
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

                return Ok(new
                {
                    id = user.Id,
                    usuario = user.UserName,
                    nombre = user.Name,
                    apellido = user.LastName,
                    correo = user.Email,
                    cedula = user.DocumentIdNumber,
                    rol = RoleTranslator.Translate(user.Role ?? ""),
                    verificado = user.IsVerified
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
