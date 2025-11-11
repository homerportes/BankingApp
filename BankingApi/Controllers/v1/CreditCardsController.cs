using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BankingApi.Controllers.v1
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]
    [Route("api/v{version::apiVersion}/creditcards")]
    public class CreditCardsController : BaseApiController
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IUserService _userService;

        public CreditCardsController(ICreditCardService creditCardService, IUserService userService)
        {
            _creditCardService = creditCardService;
            _userService = userService;
        }

        [HttpGet(Name = "ObtenerTodasLasTarjetas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, int pageSize = 20, string? estado = null)
        {
            try
            {
                var result = await _creditCardService.GetAllAsync(page, pageSize, estado);
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("search", Name = "BuscarTarjetasPorCedula")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchByDocument([FromQuery] string cedula, [FromQuery] string? estado = null)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return BadRequest("La cédula es requerida");
            }

            try
            {
                var result = await _creditCardService.GetByClientDocumentAsync(cedula, estado);
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "ObtenerTarjetaPorId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    return NotFound("La tarjeta especificada no existe");
                }

                return Ok(JsonConvert.SerializeObject(card));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}/purchases", Name = "ObtenerConsumosDeTarjeta")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPurchases([FromRoute] int id)
        {
            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    return NotFound("La tarjeta especificada no existe");
                }

                var purchases = await _creditCardService.GetPurchasesByCardIdAsync(id);
                return Ok(JsonConvert.SerializeObject(purchases));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("{clientId}", Name = "AsignarTarjetaDeCredito")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromRoute] string clientId, [FromBody] CreateCreditCardDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            var missingFields = new List<string>();

            if (dto.CreditLimitAmount <= 0)
            {
                missingFields.Add("limiteCredito");
            }

            if (missingFields.Any())
            {
                return BadRequest(new
                {
                    mensaje = "Faltan uno o más campos requeridos.",
                    camposFaltantes = missingFields
                });
            }

            try
            {
                // Verificar que el cliente existe
                var client = await _userService.GetUserById(clientId);
                if (client == null)
                {
                    return NotFound("El cliente especificado no existe");
                }

                // Verificar que el cliente tiene rol CLIENT
                if (!client.Role.Equals("CLIENT", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Solo se pueden asignar tarjetas a usuarios con rol de cliente");
                }

                // Obtener el ID del admin logueado
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized("No se pudo identificar al administrador");
                }

                var result = await _creditCardService.CreateAsync(clientId, dto.CreditLimitAmount, adminId);

                return Created($"/api/v1/creditcards/{result.Id}", JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}", Name = "ActualizarLimiteTarjeta")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCreditCardDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            var missingFields = new List<string>();

            if (dto.CreditLimitAmount <= 0)
            {
                missingFields.Add("limiteCredito");
            }

            if (missingFields.Any())
            {
                return BadRequest(new
                {
                    mensaje = "Faltan uno o más campos requeridos.",
                    camposFaltantes = missingFields
                });
            }

            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    return NotFound("La tarjeta especificada no existe");
                }

                // Verificar que el nuevo límite no sea menor que la deuda actual
                if (dto.CreditLimitAmount < card.TotalAmountOwed)
                {
                    return BadRequest("El nuevo límite no puede ser inferior al monto adeudado actual");
                }

                var updated = await _creditCardService.UpdateCreditLimitAsync(id, dto.CreditLimitAmount);
                if (!updated)
                {
                    return BadRequest("No se pudo actualizar el límite de la tarjeta");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}", Name = "CancelarTarjeta")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cancel([FromRoute] int id)
        {
            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    return NotFound("La tarjeta especificada no existe");
                }

                // Verificar que no tenga deuda pendiente
                if (card.TotalAmountOwed > 0)
                {
                    return BadRequest("Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente");
                }

                var cancelled = await _creditCardService.CancelCardAsync(id);
                if (!cancelled)
                {
                    return BadRequest("No se pudo cancelar la tarjeta");
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
