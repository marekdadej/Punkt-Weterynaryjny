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
    [Authorize]
    public class AnimalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AnimalsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Index(string ownerId = null)
        {
            var owners = await _userManager.Users
                .Where(u => u.Email != "pracownik@punktweterynaryjny.pl")
                .OrderBy(u => u.Email)
                .ToListAsync();

            ViewBag.Owners = owners;
            IQueryable<Animal> animalsQuery = _context.Animals.Include(a => a.Owner);

            if (!string.IsNullOrEmpty(ownerId))
                animalsQuery = animalsQuery.Where(a => a.OwnerId == ownerId);

            var animals = await animalsQuery.OrderBy(a => a.Name).ToListAsync();
            ViewBag.SelectedOwnerId = ownerId;
            return View(animals);
        }

        public async Task<IActionResult> MyPets()
        {
            var userId = _userManager.GetUserId(User);
            var animals = await _context.Animals
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();
            return View(animals);
        }

        [HttpGet]
        [Authorize(Roles = "Pracownik")]
        public IActionResult Add(string ownerId = null)
        {
            ViewBag.OwnerId = ownerId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Add([Bind("Name,Species,Breed,BirthDate")] Animal animal, string ownerId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OwnerId = ownerId;
                return View(animal);
            }

            animal.OwnerId = ownerId;
            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Zwierzak został dodany!";
            return RedirectToAction(nameof(Index), new { ownerId });
        }

        [HttpGet]
        [Authorize]
        public IActionResult AddForUser()
        {
            return View("Add");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddForUser([Bind("Name,Species,Breed,BirthDate")] Animal animal)
        {
            if (!ModelState.IsValid)
                return View("Add", animal);

            animal.OwnerId = _userManager.GetUserId(User);
            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Zwierzak został dodany!";
            return RedirectToAction(nameof(MyPets));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Animal animal;
            if (User.IsInRole("Pracownik"))
                animal = await _context.Animals.Include(a => a.Owner).FirstOrDefaultAsync(a => a.Id == id);
            else
            {
                var userId = _userManager.GetUserId(User);
                animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);
            }
            if (animal == null) return NotFound();
            return View(animal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Species,Breed,BirthDate")] Animal model)
        {
            Animal animal;
            if (User.IsInRole("Pracownik"))
                animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id);
            else
            {
                var userId = _userManager.GetUserId(User);
                animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);
            }
            if (animal == null) return NotFound();
            if (!ModelState.IsValid) return View(model);

            animal.Name = model.Name;
            animal.Species = model.Species;
            animal.Breed = model.Breed;
            animal.BirthDate = model.BirthDate;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dane zwierzaka zostały zaktualizowane!";
            return User.IsInRole("Pracownik") ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(MyPets));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Animal animal;
            if (User.IsInRole("Pracownik"))
                animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id);
            else
            {
                var userId = _userManager.GetUserId(User);
                animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);
            }
            if (animal == null) return NotFound();

            _context.Animals.Remove(animal);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Zwierzak został usunięty!";
            return User.IsInRole("Pracownik") ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(MyPets));
        }

        [Authorize(Roles = "Pracownik")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.Email == "pracownik@punktweterynaryjny.pl")
                return NotFound();

            var animals = await _context.Animals.Where(a => a.OwnerId == userId).ToListAsync();
            _context.Animals.RemoveRange(animals);

            await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Konto użytkownika oraz powiązane zwierzęta zostały usunięte!";
            return RedirectToAction(nameof(Index));
        }
    }
}