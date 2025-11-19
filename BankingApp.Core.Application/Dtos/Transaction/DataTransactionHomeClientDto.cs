

using BankingApp.Core.Domain.Common.Enums;

namespace BankingApp.Core.Application.Dtos.Transaction
{
    public class DataTransactionHomeClientDto
    {


        public Guid Id { get; set; }
        public DateTime Fecha {  get; set; }
        public decimal Monto { get; set; }
        public TransactionType Type { get; set; }
        public string? Beneficiary { get; set; }
        public string? Origin {  get; set; }

        public OperationStatus Status { get; set; }
        public DescriptionTransaction Description {  get; set; }


    }
}
