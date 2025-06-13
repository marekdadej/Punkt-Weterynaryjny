using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            _context      = context;
            _userManager  = userManager;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
			Console.WriteLine($"Liczba produktów: {products.Count}");
			return View(products);
        }

        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") 
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId   = product.Id,
                    ProductName = product.Name,
                    Price       = product.Price,
                    Quantity    = 1
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
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") 
                       ?? new List<CartItem>();
            return View(cart);
        }

        // GET: Checkout – wyświetlenie podsumowania + formularza
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") 
                       ?? new List<CartItem>();

            var vm = new CheckoutViewModel
            {
                CartItems = cart
            };
            return View(vm);
        }

		// POST: Checkout – finalizacja zamówienia
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
                UserId          = userId,
                OrderDate       = DateTime.Now,
                Status          = OrderStatus.Przyjęte,
                ShippingAddress = model.ShippingAddress,
                ContactPhone    = model.ContactPhone,
                OrderItems      = cart.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity  = c.Quantity,
                    Price     = c.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

			HttpContext.Session.SetObject("Cart", new List<CartItem>());

			TempData["SuccessMessage"] = "Zamówienie zostało złożone pomyślnie!";
            return RedirectToAction("Index");
        }
    }
}