using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PunktWeterynaryjny.Data;
using PunktWeterynaryjny.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PunktWeterynaryjny.Controllers
{
	[Authorize(Roles = "Właściciel")]
	public class OwnerController : Controller
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext _context;


		public OwnerController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_context = context;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		// Lista pracowników
		public async Task<IActionResult> Employees()
		{
			var users = await _userManager.GetUsersInRoleAsync("Pracownik");
			var list = new List<EmployeeViewModel>();

			foreach (var user in users)
			{
				list.Add(new EmployeeViewModel
				{
					Id = user.Id,
					Email = user.Email,
					IsLockedOut = await _userManager.IsLockedOutAsync(user)
				});
			}

			return View(list);
		}


		// Usuń konto pracownika
		[HttpPost]
		public async Task<IActionResult> DeleteEmployee(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user != null)
			{
				await _userManager.DeleteAsync(user);
				TempData["SuccessMessage"] = "Pracownik został usunięty.";
			}

			return RedirectToAction("Employees");
		}

		// Zablokuj konto (ustawienie LockoutEnd na 100 lat)
		[HttpPost]
		public async Task<IActionResult> BlockEmployee(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user != null)
			{
				await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
				TempData["SuccessMessage"] = "Pracownik został zablokowany.";
			}

			return RedirectToAction("Employees");
		}

		// Przywróć konto
		[HttpPost]
		public async Task<IActionResult> UnblockEmployee(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user != null)
			{
				await _userManager.SetLockoutEndDateAsync(user, null);
				TempData["SuccessMessage"] = "Pracownik został odblokowany.";
			}

			return RedirectToAction("Employees");
		}
		[HttpGet]
		public IActionResult CreateEmployee()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateEmployee(CreateEmployeeViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = new IdentityUser
			{
				UserName = model.Email,
				Email = model.Email,
				EmailConfirmed = true
			};

			var result = await _userManager.CreateAsync(user, model.Password);
			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, "Pracownik");
				TempData["SuccessMessage"] = "Pracownik został dodany.";
				return RedirectToAction("Employees");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error.Description);

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Dashboard()
		{
			var now = DateTime.Now;

			// Wizyty
			var visits = await _context.Visits.ToListAsync();
			ViewBag.WeeklyVisits = visits.Count(v => v.VisitDate >= now.AddDays(-7));
			ViewBag.MonthlyVisits = visits.Count(v => v.VisitDate.Month == now.Month && v.VisitDate.Year == now.Year);
			ViewBag.TotalVisits = visits.Count;
			ViewBag.OutVisits = visits.Count(v => v.IsOutVisit);
			ViewBag.RegularVisits = visits.Count(v => !v.IsOutVisit);
			ViewBag.TopHours = visits
				.GroupBy(v => v.VisitDate.ToString("HH:00"))
				.OrderByDescending(g => g.Count())
				.Select(g => new { Hour = g.Key, Count = g.Count() })
				.Take(5)
				.ToList();

			// Sprzedaż
			var orders = await _context.Orders
				.Include(o => o.OrderItems)
				.ThenInclude(i => i.Product)
				.ToListAsync();

			ViewBag.TotalOrders = orders.Count;
			ViewBag.TotalRevenue = orders.Sum(o => o.OrderItems.Sum(i => i.Quantity * i.Price));
			ViewBag.WeeklyRevenue = orders
				.Where(o => o.OrderDate >= now.AddDays(-7))
				.Sum(o => o.OrderItems.Sum(i => i.Quantity * i.Price));
			ViewBag.MonthlyRevenue = orders
				.Where(o => o.OrderDate.Month == now.Month && o.OrderDate.Year == now.Year)
				.Sum(o => o.OrderItems.Sum(i => i.Quantity * i.Price));
			ViewBag.TopProducts = orders
				.SelectMany(o => o.OrderItems)
				.GroupBy(i => i.Product.Name)
				.Select(g => new { Name = g.Key, Quantity = g.Sum(i => i.Quantity) })
				.OrderByDescending(g => g.Quantity)
				.Take(5)
				.ToList();

			return View();
		}
	}
}
