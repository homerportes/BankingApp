using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;


namespace BankingApp.Infraestructure.Identity.Seeds
{
    public class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "",
                LastName = "",
                Name = "",
                UserName = "superAdmin",
                Email = "no.repply.bankingapp@gmail.com"

            });

            var user = await  userManager.FindByNameAsync("superAdmin");
            await userManager.AddPasswordAsync(user, "Pa$Word1");

            await userManager.AddToRoleAsync(user, AppRoles.ADMIN.ToString());
        }
    }
}
