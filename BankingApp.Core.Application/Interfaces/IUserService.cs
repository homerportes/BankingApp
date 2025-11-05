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
        Task<UserPaginationResultDto> GetAllExceptCommerce(int page = 1, int pageSize = 20, string? rol = null);
    }
}
