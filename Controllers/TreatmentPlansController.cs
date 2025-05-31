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
    public class TreatmentPlansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TreatmentPlansController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Animal(int animalId)
        {
            var animal = await _context.Animals.FindAsync(animalId);
            if (animal == null)
                return NotFound();

            var plans = await _context.TreatmentPlans
                .Where(tp => tp.AnimalId == animalId)
                .OrderByDescending(tp => tp.Date)
                .ToListAsync();

            ViewBag.Animal = animal;
            return View(plans);
        }

        [Authorize(Roles = "Pracownik")]
        [HttpGet]
        public IActionResult Add(int animalId)
        {
            if (animalId == 0)
                return NotFound();
            var model = new TreatmentPlan { AnimalId = animalId };
            return View(model);
        }

        [Authorize(Roles = "Pracownik")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TreatmentPlan plan)
        {
            Console.WriteLine($"PROBA DODANIA: AnimalId={plan.AnimalId}, Date={plan.Date}, Desc={plan.Description}, IsModelValid={ModelState.IsValid}");

            if (!ModelState.IsValid || plan.AnimalId == 0)
                return View(plan);

            _context.TreatmentPlans.Add(plan);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Dodano wpis do planu leczenia.";
            return RedirectToAction(nameof(Animal), new { animalId = plan.AnimalId });
        }

        [Authorize(Roles = "Pracownik")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _context.TreatmentPlans.FindAsync(id);
            if (plan == null)
                return NotFound();
            return View(plan);
        }

        [Authorize(Roles = "Pracownik")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TreatmentPlan model)
        {
            if (id != model.Id)
                return NotFound();
            if (!ModelState.IsValid)
                return View(model);

            var plan = await _context.TreatmentPlans.FindAsync(id);
            if (plan == null)
                return NotFound();

            plan.Date = model.Date;
            plan.Description = model.Description;
            plan.Recommendations = model.Recommendations;
            plan.Medicines = model.Medicines;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wpis został zaktualizowany.";
            return RedirectToAction(nameof(Animal), new { animalId = plan.AnimalId });
        }

        [Authorize(Roles = "Pracownik")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.TreatmentPlans.FindAsync(id);
            if (plan == null)
                return NotFound();

            int animalId = plan.AnimalId;
            _context.TreatmentPlans.Remove(plan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wpis został usunięty.";
            return RedirectToAction(nameof(Animal), new { animalId });
        }
    }
}