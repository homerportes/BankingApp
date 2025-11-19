using BankingApp.Core.Application.Dtos.Commerce;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ICommerceService : IGenericService<Commerce, CommerceDto>
    {
        Task<bool> CommerceAlreadyHasUser(int CommerceId);
        Task<CommercePaginationDto> GetAllActiveFiltered(int? page, int? pageSize);
        Task<List<string>> GetCommerceAssociates(int commerceId);
        Task<OperationResultDto> SetUser(int CommerceId, string UserId);
    }
}
