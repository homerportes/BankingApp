using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UserPaginationResultDto
    {
        public required List <UserDto> UserList { get; set; }
        public int TotalCount { get; set; }
         public int CurrentPage { get; set; }
       public int PagesCount { get; set; }
    }
}
