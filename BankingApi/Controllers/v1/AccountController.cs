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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Username)|| string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }
            try
            {

                var result = await _accountService.AuthenticateAsync(loginDto);
                if(result.HasError)
                {
                    return Unauthorized("Usuario o contraseña incorrectos");
                }
                var jwt = result.AccessToken;
                return Ok(new { jwt });

            }
            catch (Exception ex)
            { 
                return StatusCode(StatusCodes. Status500InternalServerError, ex.Message);
            }

        }


    }
}
