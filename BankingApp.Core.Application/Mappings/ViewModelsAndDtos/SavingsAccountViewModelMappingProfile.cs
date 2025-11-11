using AutoMapper;
using BankingApp.Core.Application.ViewModels.SavingsAccount;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Application.Mappings.ViewModelsAndDtos
{
    public class SavingsAccountViewModelMappingProfile : Profile
    {
        public SavingsAccountViewModelMappingProfile()
        {
            // AccountDto a SavingsAccountViewModel
            CreateMap<AccountDto, SavingsAccountViewModel>()
                .ForMember(dest => dest.ClientName, opt => opt.Ignore()); // Se llenará desde el controlador

            // SaveSavingsAccountViewModel a AccountDto
            CreateMap<SaveSavingsAccountViewModel, AccountDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Number, opt => opt.Ignore()) // Se generará
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.InitialBalance))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => AccountType.SECONDARY))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AccountStatus.ACTIVE))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

            // Transaction a TransactionViewModel
            CreateMap<Transaction, TransactionViewModel>()
                .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.DateTime));
        }
    }
}
