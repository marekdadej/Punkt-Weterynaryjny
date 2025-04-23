using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PunktWeterynaryjny.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roles = { "Pracownik" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Rola '{role}' została utworzona.");
                    }
                    else
                    {
                        logger.LogError($"Błąd podczas tworzenia roli '{role}': {string.Join(", ", result.Errors)}");
                    }
                }
                else
                {
                    logger.LogInformation($"Rola '{role}' już istnieje.");
                }
            }

            string email = "pracownik@weterynaryjny.pl";
            string password = "Marek1#";  

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Pracownik");
                    logger.LogInformation($"Użytkownik '{email}' został utworzony i przypisany do roli 'Pracownik'.");
                }
                else
                {
                    logger.LogError($"Błąd podczas tworzenia użytkownika '{email}': {string.Join(", ", result.Errors)}");
                }
            }
            else
            {
                logger.LogInformation($"Użytkownik '{email}' już istnieje.");
            }
        }
    }
}