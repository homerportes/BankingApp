using AutoMapper;
using BankingApp.Core.Application.Dtos.Installment;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class InstalmentEntityMapping :Profile
    {
        public InstalmentEntityMapping()
        {
            CreateMap<Installment, InstallmentDto>();



            CreateMap<Installment, DetailsLoanHomeClientDto>()
                .ReverseMap();
           

         

        }



    }
}
