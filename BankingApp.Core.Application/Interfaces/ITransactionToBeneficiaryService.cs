using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.ViewModels.Beneficiary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ITransactionToBeneficiaryService : IGenericService<Transaction,CreateTransactionDto>
    {


        Task<List<BeneficiaryToTransactionDto>> GetLisBeneficiary(string IdCliente);
   

    }
}
