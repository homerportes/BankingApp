using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Beneficiary
{
    public class CreateBeneficiaryViewModel
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Debes de introducir un numero de cuenta")]
        public string Number { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string BeneficiaryId { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }


    }
}
