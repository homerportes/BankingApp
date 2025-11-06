using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;


namespace BankingApp.Infraestructure.Identity.Entities
{
    public class AppUser : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string DocumentIdNumber { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }= DateTime.Now;

    }
}
