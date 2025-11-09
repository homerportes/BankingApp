using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Beneficiary
{
    public class DeleteBeneficiaryViewModel
    {
        public int Id { get; set; } 
        public string   BeneficiaryId { get; set; } = string.Empty; 
    }
}
