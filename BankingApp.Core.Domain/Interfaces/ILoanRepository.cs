using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<bool> LoanPublicIdExists(string id);
        Task<Loan> UpdateByObjectAsync(Loan entity);

    }
}
