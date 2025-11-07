using BankingApp.Core.Application.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiUserPaginationResultDto> GetAllExceptCommerce(int page = 1, int pageSize = 20, string? rol = null);
        Task<ApiUserPaginationResultDto> GetAllOnlyCommerce(int page = 1, int pageSize = 20, string? rol = null);
        Task<UserDto?> GetByDocumentId(string documentId);
    }
}
