using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PunktWeterynaryjny.Data;
using PunktWeterynaryjny.Helpers;
using PunktWeterynaryjny.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PunktWeterynaryjny.Controllers
{
	public class ShopController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public ShopController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public IActionResult Index()
		{
			var products = _context.Products.ToList();
			return View(products);
		}

		[HttpPost]
		public IActionResult AddToCart(int id)
		{
			var product = _context.Products.FirstOrDefault(p => p.Id == id);
			if (product == null) return NotFound();

			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
			var item = cart.FirstOrDefault(c => c.ProductId == id);

			if (item != null)
			{
				if (product.Stock < item.Quantity + 1)
				{
					TempData["ErrorMessage"] = "Brak wystarczającej ilości produktu.";
					return RedirectToAction("Index");
				}
				item.Quantity++;
			}
			else
			{
				if (product.Stock < 1)
				{
					TempData["ErrorMessage"] = "Produkt jest niedostępny.";
					return RedirectToAction("Index");
				}

				cart.Add(new CartItem
				{
					ProductId = product.Id,
					ProductName = product.Name,
					Price = product.Price,
					Quantity = 1
				});
			}

			HttpContext.Session.SetObject("Cart", cart);
			TempData["SuccessMessage"] = "Dodano do koszyka!";
			return RedirectToAction("Index");
		}


		[HttpPost]
		public IActionResult UpdateCart(int[] productIds, int[] quantities)
		{
			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();

			for (int i = 0; i < productIds.Length; i++)
			{
				var item = cart.FirstOrDefault(c => c.ProductId == productIds[i]);
				if (item != null)
					item.Quantity = quantities[i];
			}

			HttpContext.Session.SetObject("Cart", cart);
			return RedirectToAction("Cart");
		}

		[HttpPost]
		public IActionResult RemoveFromCart(int id)
		{
			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
			var item = cart.FirstOrDefault(c => c.ProductId == id);
			if (item != null)
				cart.Remove(item);

			HttpContext.Session.SetObject("Cart", cart);
			return RedirectToAction("Cart");
		}

		public IActionResult Cart()
		{
			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
			return View(cart);
		}

		[Authorize]
		public async Task<IActionResult> Checkout()
		{
			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();

			foreach (var item in cart)
			{
				var product = await _context.Products.FindAsync(item.ProductId);
				if (product == null || product.Stock < item.Quantity)
				{
					TempData["ErrorMessage"] = $"Brak wystarczającej ilości produktu: {item.ProductName}";
					return RedirectToAction("Cart");
				}
			}

			var vm = new CheckoutViewModel
			{
				CartItems = cart
			};

			return View(vm);
		}


		[Authorize]
		[HttpPost]
		public async Task<IActionResult> Checkout(CheckoutViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
			if (!cart.Any())
			{
				TempData["ErrorMessage"] = "Koszyk jest pusty.";
				return RedirectToAction("Cart");
			}

			var userId = _userManager.GetUserId(User)!;
			var order = new Order
			{
				UserId = userId,
				OrderDate = DateTime.Now,
				Status = OrderStatus.Przyjęte,
				ShippingAddress = model.ShippingAddress,
				ContactPhone = model.ContactPhone,
				OrderItems = cart.Select(c => new OrderItem
				{
					ProductId = c.ProductId,
					Quantity = c.Quantity,
					Price = c.Price
				}).ToList()
			};

			_context.Orders.Add(order);

			// Odejmij stany magazynowe:
			foreach (var item in order.OrderItems)
			{
				var product = await _context.Products.FindAsync(item.ProductId);
				if (product != null)
				{
					product.Stock -= item.Quantity;
				}
			}

			await _context.SaveChangesAsync();

			// Wyczyść koszyk
			HttpContext.Session.SetObject("Cart", new List<CartItem>());

			TempData["SuccessMessage"] = "Zamówienie zostało złożone pomyślnie!";
			return RedirectToAction("Index");
		}


		// -------------------------------
		// Zarządzanie produktami (dla pracownika)
		// -------------------------------

		[Authorize(Roles = "Pracownik")]
		[HttpGet]
		public IActionResult CreateProduct()
		{
			return View("EditProduct", new Product());
		}

		[Authorize(Roles = "Pracownik")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateProduct(Product product)
		{
			if (!ModelState.IsValid)
				return View("EditProduct", product);

			_context.Products.Add(product);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		[Authorize(Roles = "Pracownik")]
		[HttpGet]
		public async Task<IActionResult> EditProduct(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null) return NotFound();
			return View("EditProduct", product);
		}

		[Authorize(Roles = "Pracownik")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditProduct(Product product)
		{
			if (!ModelState.IsValid)
				return View("EditProduct", product);

			_context.Products.Update(product);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		[Authorize(Roles = "Pracownik")]
		[HttpGet]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null) return NotFound();

			return View("DeleteProduct", product); // ← to musi być!
		}

		[Authorize(Roles = "Pracownik")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ConfirmDeleteProduct(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
				return NotFound();

			_context.Products.Remove(product);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "Produkt został usunięty.";
			return RedirectToAction("Index"); // lub inna akcja listująca
		}
	}
}
