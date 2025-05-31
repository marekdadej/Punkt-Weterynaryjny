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

		[Authorize]
		public IActionResult MyOrders()
		{
			var userId = _userManager.GetUserId(User);

			var orders = _context.Orders
				.Where(o => o.UserId == userId)
				.Select(o => new OrderViewModel
				{
					OrderId = o.Id,
					OrderDate = o.OrderDate,
					Status = o.Status
				})
				.ToList();

			return View(orders);
		}


		// POST: Checkout – finalizacja zamówienia
		[Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.CartItems == null || !model.CartItems.Any())
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
                OrderItems      = model.CartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity  = c.Quantity,
                    Price     = c.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");
            TempData["SuccessMessage"] = "Zamówienie zostało złożone pomyślnie!";
            return RedirectToAction("Index");
        }
    }
}