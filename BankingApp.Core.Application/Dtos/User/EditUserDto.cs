using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class EditUserDto
    {
        public string? Id { get; set; }


        public required string Password { get; set; }

        public required string Name { get; set; }

        public  string? PhoneNumber { get; set; }
        public required string LastName { get; set; }

        public  string ?ProfileImageUrl { get; set; }
    }
}
