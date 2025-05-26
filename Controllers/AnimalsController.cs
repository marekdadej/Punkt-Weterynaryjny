using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PunktWeterynaryjny.Data;
using PunktWeterynaryjny.Models;

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
        public async Task<IActionResult> Index()
        {
            var animals = await _context.Animals.Include(a => a.Owner).ToListAsync();
            return View(animals);
        }

        public async Task<IActionResult> MyPets()
        {
            var userId = _userManager.GetUserId(User);
            var animals = await _context.Animals
                .Where(a => a.OwnerId == userId)
                .ToListAsync();
            return View(animals);
        }

        [HttpGet]
        public IActionResult Add() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name,Species,Breed,BirthDate")] Animal animal)
        {
            if (!ModelState.IsValid)
                return View(animal);

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
    }
}