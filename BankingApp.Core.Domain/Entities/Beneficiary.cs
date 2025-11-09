

namespace BankingApp.Core.Domain.Entities
{
    public class Beneficiary
    {


        public int Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string BeneficiaryId { get; set; } = string.Empty;
        public DateTime Fecha {  get; set; }


    }
}
