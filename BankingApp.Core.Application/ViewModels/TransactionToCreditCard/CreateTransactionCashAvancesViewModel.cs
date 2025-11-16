using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.TransactionToCreditCard
{
    public class CreateTransactionCashAvancesViewModel
    {
       
        
        
        [Range(0.001, double.MaxValue, ErrorMessage = "El valor del monto debe ser valido y mayor a 0")]
        [Required(ErrorMessage = "Debes ingresar el valor del monto")]
        public required decimal Amount { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una tarjeta de credito")]
        public required string CreditCard { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una cuenta")]
        public required string Account { get; set; }

    }
}
