using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Login
{
    public class LoginResponseForApi
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public bool? HasError { get; set; }

        public string? AccessToken { get; set; }
        public List<string>? Errors { get; set; }


    }
}
