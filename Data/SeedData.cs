using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PunktWeterynaryjny.Data
{
	public static class SeedData
	{
		public static async Task InitializeAsync(
			UserManager<IdentityUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ILogger logger)
		{
			string[] roles = { "Pracownik", "Właściciel" };

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

			// Konto pracownika
			await CreateUserWithRoleAsync(
				userManager, logger,
				email: "pracownik@punktweterynaryjny.pl",
				password: "Marek1#",
				role: "Pracownik"
			);

			// Konto właściciela
			await CreateUserWithRoleAsync(
				userManager, logger,
				email: "wlasciciel@punktweterynaryjny.pl",
				password: "Wlasciciel123!",
				role: "Właściciel"
			);
		}

		private static async Task CreateUserWithRoleAsync(
			UserManager<IdentityUser> userManager,
			ILogger logger,
			string email,
			string password,
			string role)
		{
			var existingUser = await userManager.FindByEmailAsync(email);
			if (existingUser == null)
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
					await userManager.AddToRoleAsync(user, role);
					logger.LogInformation($"Użytkownik '{email}' został utworzony i przypisany do roli '{role}'.");
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
