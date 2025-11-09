using BankingApp.Core.Application.Dtos.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Commerce
{
    public class CommercePaginationDto
    {
        public required List<CommerceDto> Data { get; set; }

        [JsonProperty("totalComercios")]

        public int TotalCount { get; set; }
        [JsonProperty("paginaActual")]

        public int CurrentPage { get; set; }
        [JsonProperty("totalPaginas")]

        public int PagesCount { get; set; }

    }
}
