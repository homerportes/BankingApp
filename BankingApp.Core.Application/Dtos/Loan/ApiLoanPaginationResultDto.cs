using BankingApp.Core.Application.Dtos.User;
using Newtonsoft.Json;
namespace BankingApp.Core.Application.Dtos.Loan
{
    public class ApiLoanPaginationResultDto
    {
        public required List<LoanDto> Data { get; set; }


        [JsonProperty("paginaActual")]

        public int CurrentPage { get; set; }
        [JsonProperty("totalPaginas")]

        public int PagesCount { get; set; }
    }
}
