using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface IBeneficiaryRepository : IBaseRepository<Beneficiary>
    {


        Task<List<Beneficiary>> GetBeneficiariesByIdCliente(string IdCliente);

       
    }
}
