using BankingApp.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers.v1
{
    [Route("api/v{version::apiVersion}/loan")]

    public class LoansController : BaseApiController
    {
        private ILoanService _loanService;
        private IUserService _userService;
        public LoansController(ILoanService loanService, IUserService userService)
        {
            _loanService = loanService;

        }

        [HttpPost]
        public async Task<IActionResult> GetAll (int page = 1, int pageSize = 20, string? state = null, string? DocumentId = null)
        {
            string? clientId = null;

            if (!string.IsNullOrWhiteSpace(DocumentId))
            {
                var user = await _userService.GetByDocumentId(DocumentId);
                if (user == null) return BadRequest("No existe ningun usuario asociado a esa cedula");
                clientId = user.DocumentIdNumber;
            }
            var all= _loanService.GetAllFiltered(page, pageSize, state,clientId);

            return Ok(all);
        }
        
    }
}
