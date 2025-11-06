<<<<<<<< HEAD:BankingApp.Core.Application/Dtos/User/CreateUserDto.cs
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
========
﻿namespace BankingApp.Core.Application.Dtos.User
>>>>>>>> 99c2ec35db993b60848d0c8037dcd81457069c60:BankingApp.Core.Application/Dtos/User/SaveUserDto.cs
{
    public class SaveUserDto
    {
        public string? Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
        public required string DocumentIdNumber { get; set; }
        public decimal? InitialAmount { get; set; } // Monto inicial para clientes
    }
}
