using AutoMapper;
using BankingApp.Core.Application.Dtos.Installment;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.ViewModels.Installment;
using BankingApp.Core.Application.ViewModels.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.DtosAndViewModels
{
    public class LoanDtoToViewModelMappingProfile : Profile
    {

        public LoanDtoToViewModelMappingProfile()
        {
            CreateMap<LoanDto, LoanViewModel>()
                .ForMember(DEST => DEST.OutstandingBalance, opt => opt.MapFrom(src => src.OutstandingBalance))
                            .ForMember(DEST => DEST.Amount, opt => opt.MapFrom(src => src.TotalLoanAmount));

            CreateMap<DetailedLoanDto, DetailedLoanViewModel>();
            CreateMap<InstallmentDto, InstallmentViewModel>();
            CreateMap<DetailedLoanDto, EditLoanViewModel>();

            
        }
    }
}
