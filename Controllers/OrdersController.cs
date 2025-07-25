using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PunktWeterynaryjny.Data;
using PunktWeterynaryjny.Models;

namespace PunktWeterynaryjny.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

		// Wyświetlanie zamówień użytkownika
		public async Task<IActionResult> MyOrders()
		{
			var userId = _userManager.GetUserId(User);
			var orders = await _context.Orders
				.Where(o => o.UserId == userId)
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.Product)
				.OrderByDescending(o => o.OrderDate)
				.ToListAsync();

			var viewModels = orders.Select(o => new OrderViewModel
			{
				OrderId = o.Id,
				OrderDate = o.OrderDate,
				Status = o.Status,
				Items = o.OrderItems
			}).ToList();

			return View(viewModels);
		}


		// Dla pracownika: zarządzanie zamówieniami
		[Authorize(Roles = "Pracownik,Właściciel")]
		public async Task<IActionResult> Manage()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
				.OrderByDescending(o => o.OrderDate)
				.ToListAsync();

			var viewModels = orders.Select(o => new OrderViewModel
			{
				OrderId = o.Id,
				OrderDate = o.OrderDate,
				Status = o.Status,
				Items = o.OrderItems
			}).ToList();

			return View(viewModels);
        }

		// Pracownik: zmiana statusu zamówienia
		[Authorize(Roles = "Pracownik,Właściciel")]
		[HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        // Anulowanie zamówienia przez użytkownika (jeśli nie w realizacji)
        [HttpPost]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Status != OrderStatus.Przyjęte) return BadRequest();

            var userId = _userManager.GetUserId(User);
            if (order.UserId != userId) return Unauthorized();

            order.Status = OrderStatus.Anulowane;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyOrders));
        }

		[Authorize]
		public async Task<IActionResult> OrderDetails(int id)
		{
			var userId = _userManager.GetUserId(User);
			var isEmployeeOrOwner = User.IsInRole("Pracownik") || User.IsInRole("Właściciel");


			var orderQuery = _context.Orders
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.Product)
				.AsQueryable();

			if (!isEmployeeOrOwner)
			{
				// zwykły użytkownik może zobaczyć tylko swoje zamówienie
				orderQuery = orderQuery.Where(o => o.UserId == userId);
			}

			var order = await orderQuery.FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
				return NotFound();

			return View(order); // View z modelem typu Order
		}

		[Authorize(Roles = "Pracownik")]
		public async Task<IActionResult> Returns()
		{
			var orders = await _context.Orders
				.Where(o => o.Status == OrderStatus.Zwrot)
				.Include(o => o.User)
				.ToListAsync();

			var viewModels = orders.Select(o => new OrderViewModel
			{
				OrderId = o.Id,
				OrderDate = o.OrderDate,
				Status = o.Status,
				ReturnReason = o.ReturnReason,
				ClientEmail = o.User?.Email ?? "Nieznany"
			}).ToList();



			return View("Returns", viewModels);
		}

	}
}