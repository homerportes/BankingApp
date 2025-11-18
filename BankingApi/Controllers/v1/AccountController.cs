using Asp.Versioning;
using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace BankingApi.Controllers.v1
{

    [Authorize]
    [ApiVersion("1.0")]
    public class AccountController(IAccountServiceForWebApi accountServiceForWebApi) : BaseApiController
    {
        private IAccountServiceForWebApi _accountService = accountServiceForWebApi;



        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        [HttpPost("login", Name = "IniciarSesion")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }
            try
            {

                var result = await _accountService.AuthenticateAsync(loginDto);
                if (result.HasError)
                {
                    return Unauthorized("Usuario o contraseña incorrectos");
                }
                var jwt = result.AccessToken;
                return Ok(new { jwt });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        [AllowAnonymous]
        [HttpPost("confirm", Name = "ConfirmarCuenta")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            if (string.IsNullOrWhiteSpace(dto.Token))
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }



            try
            {
                var result = await _accountService.ConfirmAccountAsync(dto.Token, null, true);

                if (result.HasError)
                {
                    return BadRequest(result.Errors?.FirstOrDefault() ?? "El token es inválido");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN,COMMERCE")]
        [HttpPost("get-reset-token", Name = "ObtenerTokenRestablecimiento")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResetToken([FromBody] GetResetTokenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            if (string.IsNullOrWhiteSpace(dto.UserName))
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            try
            {
                var result = await _accountService.ForgotPasswordAsync(new ForgotPasswordRequestDto
                {
                    Username = dto.UserName,
                    Origin = null
                }, true);

                if (result.HasError)
                {
                    return BadRequest(result.Errors?.FirstOrDefault() ?? "El usuario es inválido");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN,COMMERCE")]
        [HttpPost("reset-password", Name = "RestablecerContrasena")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordApiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.UserId)) missingFields.Add("userId");
            if (string.IsNullOrWhiteSpace(dto.Token)) missingFields.Add("token");
            if (string.IsNullOrWhiteSpace(dto.Password)) missingFields.Add("password");
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword)) missingFields.Add("confirmPassword");

            if (missingFields.Any())
            {
                return BadRequest(new
                {
                    mensaje = "Faltan uno o más parámetros requeridos en la solicitud",
                    camposFaltantes = missingFields
                });
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest("Las contraseñas no coinciden");
            }

            try
            {
                var result = await _accountService.ResetPasswordAsync(new ResetPasswordRequestDto
                {
                    Id = dto.UserId,
                    Token = dto.Token,
                    Password = dto.Password
                });

                if (result.HasError)
                {
                    return BadRequest(result.Errors?.FirstOrDefault() ?? "El usuario o token es inválido");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
