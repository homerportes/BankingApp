using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;


namespace BankingApp.Infraestructure.Identity.Entities
{
    public class AppUser : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
    }
}
