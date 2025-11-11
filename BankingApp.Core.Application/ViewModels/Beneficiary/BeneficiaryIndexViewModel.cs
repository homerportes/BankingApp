using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Beneficiary
{
    public class BeneficiaryIndexViewModel
    {

        public DataBeneficiaryListViewModel ListBeneficiary { get; set; } = new();
        public CreateBeneficiaryViewModel CreateBeneficiary  { get; set; } = new();
        public bool ShowCreateModal { get; set; } = false;


    }
}
