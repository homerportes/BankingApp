using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Helpers
{

    public static class EnumTranslator
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

        public static string TranslateLoanStatus(string status, string language)
        {
            var normalized = status.ToUpperInvariant();

            return language.ToLowerInvariant() switch
            {
                "es" => normalized switch
                {
                    "PENDING" => "Pendiente",
                    "APPROVED" => "Aprobado",
                    "REJECTED" => "Rechazado",
                    "CANCELLED" => "Cancelado",
                    _ => status
                },
                "en" => normalized switch
                {
                    "al_dia" => LoanStatus.ONTIME.ToString(),
                    "retrasado" => LoanStatus.DELIQUENT.ToString(),
                  
                    _ => status
                },
                _ => status
            };
        }
    }
}