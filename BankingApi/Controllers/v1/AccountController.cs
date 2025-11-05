using Asp.Versioning;
using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Linq.Expressions;

namespace BankingApi.Controllers.v1
{

    [Authorize]
    [ApiVersion("1.0")]
    public class AccountController (IAccountServiceForWebApi accountServiceForWebApi) : BaseApiController
    {
        private IAccountServiceForWebApi _accountService= accountServiceForWebApi;



        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        [HttpPost("login")]
        public async Task< IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                return Ok(await _accountService.AuthenticateAsync(loginDto));

            }
            catch (Exception ex)
            { 
                return StatusCode(StatusCodes. Status500InternalServerError, ex.Message);
            }

        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        [HttpPost("Register")]
        public async Task<IActionResult> Login([FromBody] CreateUserDto  dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var result = await _accountService.RegisterUser(new SaveUserDto
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Password = dto.Password,
                    PhoneNumber = dto.PhoneNumber,
                    Role = dto.Role,
                    Id = "",
                },null, true);

                if (result == null)
                {
                    return BadRequest(result?.Errors);
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
