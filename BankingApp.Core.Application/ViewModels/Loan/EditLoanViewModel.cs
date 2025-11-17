using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Loan
{
    public class EditLoanViewModel
    {
             public required string PublicId { get; set; }


            [Required(ErrorMessage = "La tasa es requerida")]
            [Range(typeof(decimal), "0", "999999", ErrorMessage = "Debe ingresar un número válido mayor o igual a 0")]
            public decimal Rate { get; set; }
       

    }
}
