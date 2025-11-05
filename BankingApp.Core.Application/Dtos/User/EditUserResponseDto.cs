using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class EditUserResponseDto: UserResponseDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }

        public required string UserName { get; set; }
        public bool IsVerified { get; set; }
        public  List<string> ?Roles { get; set; }

        public required string Name { get; set; }

        public required string PhoneNumber { get; set; }
        public required string LastName { get; set; }

    }
}
