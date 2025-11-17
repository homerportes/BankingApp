using AutoMapper;
using BankingApp.Core.Application.ViewModels.CreditCard;
using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Application.ViewModels.HomeClient;

namespace BankingApp.Core.Application.Mappings.ViewModelsAndDtos
{
    public class CreditCardViewModelMappingProfile : Profile
    {
        public CreditCardViewModelMappingProfile()
        {
            // SaveViewModel a CreateDto
            CreateMap<SaveCreditCardViewModel, CreateCreditCardDto>()
                .ForMember(dest => dest.CreditLimitAmount, opt => opt.MapFrom(src => src.CreditLimit));

            // SaveViewModel a UpdateDto
            CreateMap<SaveCreditCardViewModel, UpdateCreditCardDto>()
                .ForMember(dest => dest.CreditLimitAmount, opt => opt.MapFrom(src => src.CreditLimit));

            // CreditCardDto a CreditCardViewModel
            CreateMap<CreditCardDto, CreditCardViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CardNumber, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => src.CreditLimitAmount))
                .ForMember(dest => dest.CurrentDebt, opt => opt.MapFrom(src => src.TotalAmountOwed))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.ClientName))
                .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.ExpirationDate)
                        ? DateTime.ParseExact(src.ExpirationDate, "MM/yy", System.Globalization.CultureInfo.InvariantCulture)
                        : DateTime.Now.AddYears(3)))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => (DateTime?)null))
                .ForMember(dest => dest.AvailableCredit, opt => opt.MapFrom(src => src.CreditLimitAmount - src.TotalAmountOwed));

            // PurchaseDto a PurchaseViewModel
            CreateMap<PurchaseDto, PurchaseViewModel>()
                .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.DateTime))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.AmountSpent))
                .ForMember(dest => dest.CommerceName, opt => opt.MapFrom(src => src.MerchantName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => ""))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseOperationStatus(src.Status)));



            CreateMap<DataHomeClientCreditCardDto, DataCreditCardHomeClientViewModel>()
                 .ReverseMap();




            CreateMap<DetailsCreditCardHomeClientDto, DetailsCreditCardHomeClientViewModel>()
                 .ReverseMap();

        }

        private static OperationStatus ParseOperationStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return OperationStatus.DECLINED;
            }

            if (Enum.TryParse<OperationStatus>(status, true, out var result))
            {
                return result;
            }

            return OperationStatus.DECLINED;
        }





      

              








    }





}

