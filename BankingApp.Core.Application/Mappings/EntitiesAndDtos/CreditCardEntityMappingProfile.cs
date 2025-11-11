using AutoMapper;
using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class CreditCardEntityMappingProfile : Profile
    {
        public CreditCardEntityMappingProfile()
        {
            // Mapeo de CreditCard a CreditCardDto
            CreateMap<CreditCard, CreditCardDto>()
                .ForMember(dest => dest.ExpirationDate,
                    opt => opt.MapFrom(src => src.ExpirationDate.ToString("MM/yy")))
                .ForMember(dest => dest.ClientName,
                    opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.ExpirationDate,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Purchases,
                    opt => opt.Ignore());

            // Mapeo de CreateCreditCardDto a CreditCard
            CreateMap<CreateCreditCardDto, CreditCard>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Number, opt => opt.Ignore())
                .ForMember(dest => dest.ClientId, opt => opt.Ignore())
                .ForMember(dest => dest.ExpirationDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmountOwed, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CVC, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.AdminId, opt => opt.Ignore())
                .ForMember(dest => dest.Purchases, opt => opt.Ignore());

            // Mapeo de Purchase a PurchaseDto
            CreateMap<Purchase, PurchaseDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CardNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreditCard, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }
}
