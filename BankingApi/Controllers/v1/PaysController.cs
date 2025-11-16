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

        public PaysController(IUserService userService, IPaymentService paymentService)
        {
            _userService = userService;
            _paymentService = paymentService;
        }
        [HttpPost("process-payment/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public async Task<IActionResult> ProcessPayment([FromRoute] int? commerceId, [FromBody] PaymentRequestDto request)
        {
            // Validaciones iniciales
            if (request == null)
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");

            if (commerceId.HasValue && commerceId <= 0)
                return BadRequest("El ID del comercio es inválido.");

            if (string.IsNullOrWhiteSpace(request.CardNumber) || request.CardNumber.Length != 16 || !request.CardNumber.All(char.IsDigit))
                return BadRequest("El número de tarjeta debe contener 16 dígitos y solo números.");

            if (request.MonthExpirationCard < 1 || request.MonthExpirationCard > 12)
                return BadRequest("El mes de vencimiento es inválido (debe estar entre 01 y 12).");

            if (request.YearExpirationCard < 2000 || request.YearExpirationCard > 9999)
                return BadRequest("El año de vencimiento es inválido (debe tener 4 dígitos).");

            if (string.IsNullOrWhiteSpace(request.Cvc) || request.Cvc.Length != 3 || !request.Cvc.All(char.IsDigit))
                return BadRequest("El CVC es inválido (debe tener 3 dígitos numéricos).");

            if (request.TransactionAmount <= 0)
                return BadRequest("El monto de la transacción debe ser mayor a 0.");

            var currentUser = await _userService.GetCurrentUserAsync();
            var userRole = EnumMapper<AppRoles>.FromString(currentUser!.Role);
            int commerceIdToUse;

            if (userRole == AppRoles.ADMIN)
            {
                if (!commerceId.HasValue)
                    return BadRequest("El identificador del comercio es requerido para rol administrador.");

                commerceIdToUse = commerceId.Value;
            }
            else
            {
                // Obtener de jwt
                commerceIdToUse = 0; 
            }

            try
            {
                var payResult = await _paymentService.Pay(request, commerceIdToUse);

                if (payResult.IsCompleted)
                    return NoContent();

                if (!string.IsNullOrWhiteSpace(payResult.Message))
                    return BadRequest(payResult.Message);

                return BadRequest("Pago no realizado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al procesar el pago.");
            }
        }



    }


    

}
