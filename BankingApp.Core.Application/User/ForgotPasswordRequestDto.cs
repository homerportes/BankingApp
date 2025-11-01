using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.User
{
    public class ForgotPasswordRequestDto
    {
        public required string Username {  get; set; }
        public required string Origin { get; set; }
    }
}
