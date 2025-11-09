

using AutoMapper;
using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.ViewModels.Beneficiary;

namespace BankingApp.Core.Application.Mappings.DtosAndViewModels
{
    public class BeneficiaryDtoAndViewModelsMappingProfile : Profile
    {




        public BeneficiaryDtoAndViewModelsMappingProfile()
        {


            CreateMap<DataBeneficiaryDto, DataBeneficiaryViewModel>()
                .ReverseMap();

            CreateMap<CreateBeneficiaryDto, CreateBeneficiaryViewModel>()
                .ForMember(c => c.Number, opt => opt.Ignore())
                .ReverseMap();


            CreateMap<CreateBeneficiaryDto, DeleteBeneficiaryViewModel>()
             .ReverseMap();




        }  


    }
}
