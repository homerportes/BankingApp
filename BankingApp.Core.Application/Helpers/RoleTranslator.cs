using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Helpers
{

    public static class RoleTranslator
    {
        public static string Translate(string roleName)
        {
            return roleName.ToUpper() switch
            {
                "ADMIN" => "Administrador",
                "CLIENT" => "Cliente",
                "TELLER" => "Cajero",
                "COMMERCE" => "Comercio",
                _ => roleName
            };
        }
    }
}