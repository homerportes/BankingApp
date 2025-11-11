using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Beneficiary
{
    public class ValidateAcountNumberExistResponseDto
    {

        public string IdBeneficiary { get; set; } = string.Empty;
        public string NameBeneficiary { get; set; } = string.Empty;
        public bool IsExist { get; set; }

    }
}
