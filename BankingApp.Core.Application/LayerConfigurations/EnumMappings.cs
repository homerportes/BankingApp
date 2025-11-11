using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Domain.Common.Enums;


namespace BankingApp.Core.Application.LayerConfigurations
{
    public class EnumMappings
    {
        public static void Initialize()
        {



            EnumMapper<AppRoles>.AddAliases(new()
            {
                { "administrador", AppRoles.ADMIN },
                { "admin", AppRoles.ADMIN },

                { "cajero", AppRoles.TELLER },
                { "teller", AppRoles.TELLER },

                { "cliente", AppRoles.CLIENT },
                { "client", AppRoles.CLIENT },
                { "comercio", AppRoles.COMMERCE },

                { "commerce", AppRoles.COMMERCE },

            });


            EnumMapper<LoanStatus>.AddAliases(new()
            {
                { "al_dia", LoanStatus.ONTIME },
                { "ontime", LoanStatus.ONTIME },
                { "atrasado", LoanStatus.DELIQUENT },
                { "deliquent", LoanStatus.DELIQUENT },

            });

            EnumMapper<AccountType>.AddAliases(new()
            {
                { "primary", AccountType.PRIMARY },
                { "primario", AccountType.PRIMARY },
                { "secondary", AccountType.SECONDARY },
                { "secundaria", AccountType.SECONDARY },


            });

            EnumMapper<CardStatus>.AddAliases(new()
            {
                { "activa", CardStatus.ACTIVE },
                 { "active", CardStatus.ACTIVE },
                { "cancelled", CardStatus.CANCELLED },
                { "cancelada", CardStatus.CANCELLED },
                { "inactive", CardStatus.CANCELLED },
                { "inactiva", CardStatus.CANCELLED },



            });

            EnumMapper<OperationStatus>.AddAliases(new()
            {
                { "declinada", OperationStatus.DECLINED },
                 { "declined", OperationStatus.DECLINED },
                { "rechazada", OperationStatus.DECLINED },
                 { "rejected", OperationStatus.DECLINED },
                { "aceptada", OperationStatus.APPROVED},
                { "aprobada", OperationStatus.APPROVED },
                { "approved", OperationStatus.APPROVED },
            });


            EnumMapper<TransactionType>.AddAliases(new()
            {
                { "credito", TransactionType.CREDIT },
                { "debito", TransactionType.DEBIT },
                { "credit", TransactionType.CREDIT },
                { "debit", TransactionType.DEBIT },
            });

        }

    }
}

