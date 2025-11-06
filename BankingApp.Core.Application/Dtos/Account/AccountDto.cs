using BankingApp.Core.Domain.Common.Enums;


namespace BankingApp.Core.Application.Dtos.Account
{
    public class AccountDto
    {
        public required int Id { get; set; }

        public required string Number { get; set; }

        public required string ClientId { get; set; }
        public Decimal Balance { get; set; }
        public AccountType Type { get; set; }
    }
}
