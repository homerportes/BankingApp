using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class CreateUserDto
    {
        public required string UserName { get; set; }

        public required string Email { get; set; }

        public required string Name { get; set; }

        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }

        public required string Role { get; set; }
    }
}
