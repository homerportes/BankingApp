using BankingApp.Core.Application.Dtos.Payment;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace BankingApi.Controllers.v1
{

    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN,COMMERCE")]

    [Route("api/v{version::apiVersion}/pay")]

    public class PaysController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly ICommerceService _commerceService;

        public PaysController(IUserService userService, IPaymentService paymentService, ICommerceService commerceService)
        {
            _userService = userService;
            _paymentService = paymentService;
            _commerceService = commerceService;
        }

        [HttpGet("get-transactions/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]

        public async Task<IActionResult> GetAllTransactions([FromRoute] int? commerceId, int page=1, int pageSize = 20)
        {


            var user = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var currentUser = await _userService.GetUserByName(user);
            var userRole = EnumMapper<AppRoles>.FromString(currentUser.Role);
            int commerceIdToUse;

            if (userRole == AppRoles.ADMIN)
            {
                if (!commerceId.HasValue)
                    return BadRequest("El ID del comercio es requerido para administradores.");

                if (commerceId <= 0)
                    return BadRequest("El ID del comercio es inválido.");

                var commerce = await _commerceService.GetByIdAsync(commerceId ?? 0);
                if (commerce == null) return BadRequest("El id proporcionado no está asociado a ningún comercio");
                commerceIdToUse = commerceId.Value;


            }
            else
            {

                var commerceIdFromToken = User.FindFirst("commerceId")?.Value;

                if (string.IsNullOrWhiteSpace(commerceIdFromToken))
                    return Unauthorized("El usuario no está asociado a ningún comercio.");

                commerceIdToUse = int.Parse(commerceIdFromToken);
            }

            var data = await _paymentService.GetTransactionsForCommerceId(commerceIdToUse, page, pageSize);

            return Ok(data);

        }







        [HttpPost("process-payment/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public async Task<IActionResult> ProcessPayment([FromRoute] int? commerceId, [FromBody] PaymentRequestDto request)
        {
            if (request == null)
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            var user = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var currentUser = await _userService.GetUserByName(user);
            var userRole = EnumMapper<AppRoles>.FromString(currentUser.Role);

            // Validaciones generales de pago
            if (string.IsNullOrWhiteSpace(request.CardNumber) || request.CardNumber.Length != 16 || !request.CardNumber.All(char.IsDigit))
                return BadRequest("El número de tarjeta debe contener 16 dígitos.");

            if (request.MonthExpirationCard < 1 || request.MonthExpirationCard > 12)
                return BadRequest("Mes de vencimiento inválido.");

            if (request.YearExpirationCard < 2000 || request.YearExpirationCard > 9999)
                return BadRequest("Año de vencimiento inválido.");

            if (string.IsNullOrWhiteSpace(request.Cvc) || request.Cvc.Length != 3 || !request.Cvc.All(char.IsDigit))
                return BadRequest("CVC inválido.");

            if (request.TransactionAmount <= 0)
                return BadRequest("Monto inválido.");

            int commerceIdToUse;


            if (userRole == AppRoles.ADMIN)
            {
                if (!commerceId.HasValue)
                    return BadRequest("El ID del comercio es requerido para administradores.");

                if (commerceId <= 0)
                    return BadRequest("El ID del comercio es inválido.");

                var commerce=await _commerceService.GetByIdAsync(commerceId??0);
                if (commerce == null) return BadRequest("El id proporcionado no está asociado a ningún comercio");
                commerceIdToUse = commerceId.Value;


            }
            else
            {
       
                var commerceIdFromToken = User.FindFirst("commerceId")?.Value;

                if (string.IsNullOrWhiteSpace(commerceIdFromToken))
                    return Unauthorized("El usuario no está asociado a ningún comercio.");

                commerceIdToUse = int.Parse(commerceIdFromToken);
            }

            try
            {
                var payResult = await _paymentService.Pay(request, commerceIdToUse);

                if (payResult.IsCompleted)
                    return NoContent();

                return BadRequest(payResult.Message ?? "Pago no realizado.");
            }
            catch
            {
                return StatusCode(500, "Error al procesar el pago.");
            }
        }




    }




}
