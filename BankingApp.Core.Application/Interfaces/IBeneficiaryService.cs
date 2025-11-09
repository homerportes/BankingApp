

using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Domain.Entities;
using System.Security.Principal;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IBeneficiaryService : IGenericService<Beneficiary,CreateBeneficiaryDto>
    {



        Task<List<DataBeneficiaryDto>> GetBeneficiaryList(string idUser);

        Task<ValidateAcountNumberExistResponseDto> ValidateAccountNumberExist(string numeber);
        Task<bool> ValidateAccountNumber(string numeber, string idCliente);


    }
}
