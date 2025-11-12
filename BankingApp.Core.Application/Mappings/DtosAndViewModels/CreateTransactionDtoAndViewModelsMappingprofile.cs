

using AutoMapper;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.ViewModels.TransaccionExpres;

namespace BankingApp.Core.Application.Mappings.DtosAndViewModels
{
    public class CreateTransactionDtoAndViewModelsMappingprofile : Profile
    {



        public CreateTransactionDtoAndViewModelsMappingprofile()
        {




            CreateMap<CreateTransactionDto, CreateTransactionExpressViewModel>()
                 .ReverseMap()
                 .ForMember(s => s.Status, opt => opt.Ignore())
                 .ForMember(s => s.DateTime, opt => opt.Ignore())
                 .ForMember(s => s.AccountId, opt => opt.Ignore())
                 .ForMember(s => s.Description, opt => opt.Ignore())
                 .ForMember(s => s.Type, opt => opt.Ignore())
                 .ForMember(s => s.AccountNumber, opt => opt.Ignore());


            CreateMap<ValidateAccountNumberResponseDto, DataBeneficiaryExpressViewModel>()
                .ForMember(s => s.IdBeneficiary, opt => opt.MapFrom(src => src.BeneficiaryId));






        }


    }
}
