using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Common.Enums
{
    public enum AppRoles
    {
        [Display(Name = "Administrador")]
        ADMIN =1,
        [Display(Name = "Cajero")]

        TELLER,
        [Display(Name = "Cliente")]

        CLIENT,
        [Display(Name = "Comerciante")]

        COMMERCE

    }
}
