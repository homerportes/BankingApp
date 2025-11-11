using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Beneficiary
{
    public class DataBeneficiaryDto 
    {

        public int Id { get; set; }
        public string? IdBeneficiary {  get; set; }
        public required  string Name { get; set; }
        public required  string LastName { get; set; }
        public required  string Number { get; set; }


    }
}
