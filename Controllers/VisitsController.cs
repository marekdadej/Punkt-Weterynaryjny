using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PunktWeterynaryjny.Data;
using PunktWeterynaryjny.Models;
using System.Threading.Tasks;

namespace PunktWeterynaryjny.Controllers
{
    [Authorize]
    public class VisitsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VisitsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Schedule()
        {
            var userId = _userManager.GetUserId(User);
            var pets = await _context.Animals.Where(a => a.OwnerId == userId).ToListAsync();
            ViewBag.Pets = pets;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Schedule(Visit model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = _userManager.GetUserId(User);
                model.Status = VisitStatus.Zarejestrowana;
                _context.Visits.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Wizyta została umówiona!";
                return RedirectToAction("MyVisits");
            }
            return View(model);
        }

        public async Task<IActionResult> MyVisits()
        {
            var userId = _userManager.GetUserId(User);
            var visits = await _context.Visits
                .Include(v => v.Pet)
                .Where(v => v.UserId == userId)
                .ToListAsync();
            return View(visits);
        }
    }
}