

namespace BankingApp.Core.Application.ViewModels.Beneficiary
{
    public class DataBeneficiaryViewModel
    {


        public int Id { get; set; }
        public string? IdBeneficiary { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Number { get; set; }


    }
}
