using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.User
{
    public class UserResponseDto
    {
        public bool HasError { get; set; }
        public string? Error { get; set; }

    }
}
