using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;


namespace BankingApp.Infraestructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(AppRoles.ADMIN.ToString()));
            await roleManager.CreateAsync(new IdentityRole(AppRoles.CLIENT.ToString()));
            await roleManager.CreateAsync(new IdentityRole(AppRoles.TELLER.ToString()));
            await roleManager.CreateAsync(new IdentityRole(AppRoles.COMMERCE.ToString()));

        }
    }
}
