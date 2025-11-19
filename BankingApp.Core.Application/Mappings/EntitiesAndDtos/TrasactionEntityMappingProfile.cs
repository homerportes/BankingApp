
using AutoMapper;
using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using System.Transactions;
using Transaction = BankingApp.Core.Domain.Entities.Transaction;

namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class TrasactionEntityMappingProfile : Profile
    {

        public TrasactionEntityMappingProfile()
        {



            CreateMap<Transaction, CreateTransactionDto>()
                .ReverseMap()
                .ForMember(s => s.Account, opt => opt.Ignore());


            CreateMap<ValidateAcountNumberExistResponseDto, ValidateAccountNumberResponseDto>()
                .ForMember(s => s.Name, opt => opt.MapFrom(src => src.NameBeneficiary))
                .ForMember(s => s.BeneficiaryId, opt => opt.MapFrom(src => src.IdBeneficiary))
                .ForMember(s => s.IsExist, opt => opt.MapFrom(src => src.IsExist))
                .ForMember(s => s.LastName, opt => opt.Ignore())

            CreateMap<Transaction, CommerceTransactionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EnumMapper<OperationStatus>.ToString(src.Status)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => EnumMapper<TransactionType>.ToString(src.Type)));






            CreateMap<Transaction, DataTransactionHomeClientDto>()
                .ForMember(s => s.Fecha, opt => opt.MapFrom(src => src.DateTime))
                .ForMember(s => s.Monto, opt => opt.MapFrom(src => src.Amount))
                .ReverseMap();
             


        }


    }
}
