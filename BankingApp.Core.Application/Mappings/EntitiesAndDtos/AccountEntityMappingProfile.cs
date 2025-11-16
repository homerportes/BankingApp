using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.ViewModels.HomeClient;
using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class AccountEntityMappingProfile :Profile
    {
        public AccountEntityMappingProfile()
        {
            CreateMap<Account, AccountDto>()
                .ReverseMap();

            CreateMap<Account, DataAccountaHomeClientDto>()
                .ForMember(s => s.Banlace, opt => opt.MapFrom(src => src.Balance))
              .ReverseMap();



            CreateMap<DataAccountaHomeClientDto, DataAccountHomeClientViewModel>()
                .ForMember( s => s.Banlace, opt => opt.MapFrom(src => src.Banlace))
            .ReverseMap();

        }
    }
}
