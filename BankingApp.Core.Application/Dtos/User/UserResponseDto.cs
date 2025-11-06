using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UserResponseDto
    {
        public bool HasError { get; set; }
        public List<string>? Errors { get; set; }
        public string? Message { get; set; }

    }
}
