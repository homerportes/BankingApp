using AutoMapper;
using BankingApp.Core.Application.Dtos.Commerce;
using BankingApp.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace BankingApi.Controllers.v1
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]
    [Route("api/v{version::apiVersion}/commerce")]
    
    public class CommerceController : Controller
    {
        private readonly ICommerceService _commerceService;
        private readonly IMapper _mapper ;
        public CommerceController(ICommerceService commerceService, IMapper mapper)
        {
            _commerceService = commerceService;
            _mapper = mapper;
        }
        [HttpGet(Name = "ObtenerTodosLoscomercios")]
        public async Task<IActionResult> GetAll([FromQuery] int ?page, [FromQuery] int ?pageSize)
        {
            try
            {
                var result = await _commerceService.GetAllFiltered(page, pageSize);
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }




        }

        [HttpGet("/{id}")]
        public async Task<IActionResult> GetAll([FromRoute] int id)
        {
            try
            {
                var result = await _commerceService.GetByIdAsync(id);
                if (result == null) return NotFound("Comercio no encontrado");
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreateCommerceDto dto)
        {

            if (string.IsNullOrEmpty(dto.Name)) return BadRequest("El nombre es requerido");
            if (string.IsNullOrEmpty(dto.Description)) return BadRequest("La descripcion es requerida");
            if (string.IsNullOrEmpty(dto.Logo)) return BadRequest("El logo es requerido");


            try
            {

            
                var result = await _commerceService.AddAsync(_mapper.Map<CommerceDto> (dto));

                return Created();

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }


        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EditCommerceDto dto)
        {

            if (string.IsNullOrEmpty(dto.Name)) return BadRequest("El nombre es requerido");
            if (string.IsNullOrEmpty(dto.Description)) return BadRequest("La descripcion es requerida");
            if (string.IsNullOrEmpty(dto.Logo)) return BadRequest("El logo es requerido");


            try
            {

                var commerce = await _commerceService.GetByIdAsync(dto.Id);
                if (commerce == null)
                    return NotFound("Comercio no encontrado");

                if (!string.IsNullOrWhiteSpace(dto.Logo))
                    commerce.Logo = dto.Logo;

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    commerce.Description = dto.Description;

                if (!string.IsNullOrWhiteSpace(dto.Name))
                    commerce.Name = dto.Name;

                var result = await _commerceService.UpdateAsync(dto.Id, _mapper.Map<CommerceDto>(commerce));
                return NoContent();

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }





            //Preguntar es contradictorio
            /*: desactiva o activa un comercio por su identificador, cuando se 
            desactiva un comercio todos los usuarios asociado a ese comercio*/



        }
            [HttpPatch("/{id}")]
            public async Task<IActionResult> ToogleState([FromRoute] int id, [FromBody] bool status)
            {


                try
                {

                    var commerce = await _commerceService.GetByIdAsync(id);
                    if (commerce == null)
                        return NotFound("Comercio no encontrado");

                  commerce.IsActive = status;
                    var result = await _commerceService.UpdateAsync(id, _mapper.Map<CommerceDto>(commerce));
               
                    return NoContent();

                }
                catch
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }


            
            }
    }
}
