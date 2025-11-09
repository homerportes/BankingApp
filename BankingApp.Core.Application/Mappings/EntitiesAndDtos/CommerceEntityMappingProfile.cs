using AutoMapper;
using BankingApp.Core.Application.Dtos.Commerce;
using BankingApp.Core.Domain.Entities;


namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class CommerceEntityMappingProfile : Profile
    {
        public CommerceEntityMappingProfile()
        {
            CreateMap<Commerce, CommerceDto>();
        }
    }
}
