using AutoMapper;
using BankingApp.Core.Application.Dtos.Transaction.Transference;
using BankingApp.Core.Application.ViewModels.Transferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.ViewModelsAndDtos
{
    public class TransferenceRequestViewModelMappingProfile: Profile
    {
        public TransferenceRequestViewModelMappingProfile()
        {
            CreateMap<TransferenceOperationViewModel, TransferenceRequestDto>();
        }
    }
}
