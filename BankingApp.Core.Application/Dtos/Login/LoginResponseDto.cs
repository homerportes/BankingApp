using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Login
{
    public class LoginResponseDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }

        public List<string>? Roles { get; set; }
        public required string UserName { get; set; }
        public bool IsVerified { get; set; }
        public bool HasError { get; set; }
        public string ? Error {  get; set; }
    }
}
