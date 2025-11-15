using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace BankingApi.Controllers.v1
{
    [Route("api/v{version::apiVersion}/loan")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]

    public class LoansController : BaseApiController
    {
        private readonly ILoanServiceForWebApi _loanService;
        private readonly IUserService _userService;
        
        public LoansController(ILoanServiceForWebApi loanService, IUserService userService)
        {
            _loanService = loanService;
            _userService = userService;
        }

        [HttpGet(Name = "GetAllLoans")]

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll (int page = 1, int pageSize = 20, string? state = null, string? DocumentId = null)
        {
            string? clientId = null;

            if (!string.IsNullOrWhiteSpace(DocumentId))
            {
                var user = await _userService.GetByDocumentId(DocumentId);
                if (user == null) return BadRequest("No existe ningun usuario asociado a esa cedula");
                clientId = user.Id;
            }
            var all=await  _loanService.GetAllFiltered(page, pageSize, state,clientId);

            return Ok(all);
        }


        [HttpPost(Name = "CreateLoan")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status201Created)]

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateLoan(LoanRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            if (string.IsNullOrWhiteSpace(request.ClientId))
            {
                return BadRequest("El ID del usuario es requerido");
            }
            if (request.LoanAmount <= 0)
                return BadRequest("El prestamos debe ser masyor a 0");

                    if (request.AnualInterest <= 0)
                return BadRequest("El prestamos debe ser masyor a 0");
            if (request.LoanTermInMonths < 6)
                return BadRequest("El prestamos debe ser masyor a 0");
            var user = await _userService.GetUserById(request.ClientId);
            if (user == null) return BadRequest("No existe ningun cliente asociado a ese Id");

            
                var requestResult = await _loanService.HandleCreateRequestApi(request);
                if (requestResult.ClientHasActiveLoan) return BadRequest("El usuario ya tiene un prestamo activo");
                if (requestResult.ClientIsHighRisk) return Conflict("El usuario es de alto riesgo");
            if (requestResult.LoanCreated) return Created();

            else return BadRequest();
            
        }


        [HttpGet("{id}", Name = "GetLoanDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDetails([FromRoute]string id)
        {

            var result = await _loanService.GetDetailed(id);
            if (result == null) return NotFound();

            return Ok(result);
        }


        [HttpPatch("{id}/rate", Name = "UpdateLoanRate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SetRate([FromRoute]string id, [FromBody] UpdateLoanRateDto dto)
        {

            if (dto.Rate <= 0 || dto.Rate==null) return BadRequest("Tasa invalida");

            
            if (string.IsNullOrEmpty(id) || id == "string")
            {
                return BadRequest();
            }
            var result = await _loanService.UpdateLoanRate(id, dto.Rate);
            if (!result.IsSuccessful) return NotFound();
            if (result.IsSuccessful) return NoContent();
            if (result == null) return NotFound();

            return Ok(result);
        }
    }
}
