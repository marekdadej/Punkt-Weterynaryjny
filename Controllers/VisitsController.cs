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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(Visit model)
        {
            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                model.UserId = userId;
                model.Status = VisitStatus.Zarejestrowana;
                model.IsOutVisit = false;
                model.OutVisitAddress = null;
                _context.Visits.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Wizyta została umówiona!";
                return RedirectToAction("MyVisits");
            }

            ViewBag.Pets = await _context.Animals.Where(a => a.OwnerId == userId).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ScheduleOut()
        {
            var userId = _userManager.GetUserId(User);
            var pets = await _context.Animals.Where(a => a.OwnerId == userId).ToListAsync();
            ViewBag.Pets = pets;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleOut(Visit model)
        {
            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                model.UserId = userId;
                model.Status = VisitStatus.Zarejestrowana;
                model.IsOutVisit = true;
                _context.Visits.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Wyjazdowa wizyta została umówiona!";
                return RedirectToAction("MyVisits");
            }

            ViewBag.Pets = await _context.Animals.Where(a => a.OwnerId == userId).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> MyVisits()
        {
            var userId = _userManager.GetUserId(User);
            var visits = await _context.Visits
                .Include(v => v.Pet)
                .Where(v => v.UserId == userId)
                .OrderBy(v => v.VisitDate)
                .ToListAsync();
            return View(visits);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var visit = await _context.Visits
                .Include(v => v.Pet)
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

            if (visit == null || visit.Status != VisitStatus.Zarejestrowana)
                return NotFound();

            var pets = await _context.Animals.Where(a => a.OwnerId == userId).ToListAsync();
            ViewBag.Pets = pets;
            return View(visit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Visit model)
        {
            var userId = _userManager.GetUserId(User);
            if (id != model.Id)
                return NotFound();

            var visit = await _context.Visits.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            if (visit == null || visit.Status != VisitStatus.Zarejestrowana)
                return NotFound();

            if (ModelState.IsValid)
            {
                visit.PetId = model.PetId;
                visit.VisitDate = model.VisitDate;
                visit.Description = model.Description;
                visit.OutVisitAddress = model.OutVisitAddress;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Wizyta została zaktualizowana!";
                return RedirectToAction("MyVisits");
            }

            var pets = await _context.Animals.Where(a => a.OwnerId == userId).ToListAsync();
            ViewBag.Pets = pets;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var visit = await _context.Visits.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

            if (visit == null || visit.Status != VisitStatus.Zarejestrowana)
                return NotFound();

            visit.Status = VisitStatus.Anulowana;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Wizyta została anulowana.";
            return RedirectToAction("MyVisits");
        }

		[HttpGet]
		public async Task<IActionResult> GetAvailableHours(DateTime date)
		{
			var busy = await _context.Visits
				.Where(v => v.VisitDate.Date == date.Date)
				.Select(v => v.VisitDate.TimeOfDay)
				.ToListAsync();

			var all = Enumerable.Range(8, 9) // 8:00–16:30
				.SelectMany(h => new[] { new TimeSpan(h, 0, 0), new TimeSpan(h, 30, 0) })
				.Where(t => t < new TimeSpan(17, 0, 0))
				.ToList();

			var available = all.Except(busy).ToList();

			return Json(available.Select(t => t.ToString(@"hh\:mm")));
		}

	}
}