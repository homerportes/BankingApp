

namespace BankingApp.Core.Application.ViewModels.HomeClient
{
    public class DataAccountHomeClientListViewModel
    {


        public List<DataAccountHomeClientViewModel> ListAccountClient { get; set; } = new();
        public List<DataLoanHomeClientViewModel> ListLoanHomeClient { get; set; } = new();
        public List<DataCreditCardHomeClientViewModel> ListCredtiCardHomeClient { get; set; } = new();


    }
}
