using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Email
{
    public class EmailConfirmRequestDto
    {
        public  required    string Id { get; set; }
        public required string Token { get; set; }
    }
}
