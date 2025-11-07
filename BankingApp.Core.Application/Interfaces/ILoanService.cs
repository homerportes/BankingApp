using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ILoanService : IGenericService<Loan,LoanDto>
    {
        Task<ApiLoanPaginationResultDto> GetAllFiltered(int page = 1, int pageSize = 20, string? state = null ,string ? documentId = null);

    }
}
