using AutoMapper;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.ViewModels.HomeClient;
using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class LoanEntityMappingProfile: Profile

    {
        public LoanEntityMappingProfile()
        {
            CreateMap<Loan, LoanDto>()
                .ForMember(r => r.TotalInstallmentsCount, opt => opt.MapFrom(src => src.Installments.Count()))
                .ForMember(r=>r.InterestRate, opt=>opt.MapFrom(src=>src.InterestRate))
                .ForMember(r => r.TotalLoanAmount, opt => opt.MapFrom(src => src.Amount))


                .ForMember(r => r.TotalInstallmentsCount, opt => opt.MapFrom(src => src.Installments!.Count()))
           .ForMember(r => r.PaidInstallmentsCount, opt => opt.MapFrom(src => src.Installments!.Where(r => r.IsPaid).Count()));


            CreateMap<Loan, DataLoanHomeClientDto>()
                 .ForMember(s => s.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(s => s.LoanedAmountTotal, opt => opt.MapFrom(src => src.Amount))
                 .ForMember(s => s.OutstandingBalance, opt => opt.MapFrom(src => src.OutstandingBalance))
                 .ForMember(s => s.InterestRate, opt => opt.MapFrom(src => src.InterestRate))
                 .ForMember(s => s.LoanTermInMonths, opt => opt.MapFrom(src => src.LoanTermInMonths))
                 .ForMember(s => s.Number, opt => opt.MapFrom(src => src.PublicId));



            CreateMap<DataLoanHomeClientDto, DataLoanHomeClientViewModel>()
                .ReverseMap();


            CreateMap<DetailsLoanHomeClientDto, DetailsLoanHomeClientViewModel>()
                .ReverseMap();



        }
    }
}
