using BankingApp.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers.v1
{

    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]

    public class UsersController : Controller
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }


        /*
        [HttpGet]
        public async Task<IActionResult> GetAll([FromBody] int  page=1, int pageSize=20, string? rol=null)
        {
            var result = await _userService.GetAllExceptCommerce(page, pageSize, rol);


        }
        */

    }
}
