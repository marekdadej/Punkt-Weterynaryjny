using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PunktWeterynaryjny.Data;
using PunktWeterynaryjny.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunktWeterynaryjny.Controllers
{
    [Authorize(Roles = "Pracownik,Właściciel")]
    public class MyVisitsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyVisitsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

		public async Task<IActionResult> Index(string ownerId = null, int? petId = null, string email = null)
		{
			var owners = await _userManager.Users
				.Where(u => u.Email != "pracownik@punktweterynaryjny.pl")
				.OrderBy(u => u.Email)
				.ToListAsync();

			ViewBag.Owners = owners;
			ViewBag.SelectedOwnerId = ownerId;

			// Jeśli podano email i NIE wybrano ownerId — szukamy użytkownika po e-mailu
			if (!string.IsNullOrEmpty(email) && string.IsNullOrEmpty(ownerId))
			{
				var user = await _userManager.FindByEmailAsync(email);
				if (user != null)
				{
					ownerId = user.Id;
					ViewBag.SelectedOwnerId = user.Id;

					if (!owners.Any(o => o.Id == user.Id))
					{
						owners.Insert(0, user); // dodaj do listy rozwijanej
					}
				}
				else
				{
					ViewBag.NotFoundMessage = $"Nie znaleziono użytkownika o adresie e-mail: {email}";
				}

				ViewBag.Email = ""; // zawsze czyść pole
			}

			IQueryable<Animal> petsQuery = _context.Animals.Include(a => a.Owner);
			if (!string.IsNullOrEmpty(ownerId))
				petsQuery = petsQuery.Where(a => a.OwnerId == ownerId);

			var pets = await petsQuery.OrderBy(a => a.Name).ToListAsync();
			ViewBag.Pets = pets;
			ViewBag.SelectedPetId = petId;

			IQueryable<Visit> visitsQuery = _context.Visits
				.Include(v => v.Pet)
				.ThenInclude(p => p.Owner);

			if (!string.IsNullOrEmpty(ownerId))
				visitsQuery = visitsQuery.Where(v => v.Pet.OwnerId == ownerId);
			if (petId.HasValue)
				visitsQuery = visitsQuery.Where(v => v.PetId == petId.Value);

			var visits = await visitsQuery
				.OrderByDescending(v => v.VisitDate)
				.ToListAsync();

			return View(visits);
		}


		[HttpGet]
        public async Task<IActionResult> Add(string ownerId = null, int? petId = null)
        {
            var owners = await _userManager.Users
                .Where(u => u.Email != "pracownik@punktweterynaryjny.pl")
                .OrderBy(u => u.Email)
                .ToListAsync();
            ViewBag.Owners = owners;
            ViewBag.SelectedOwnerId = ownerId;

            IQueryable<Animal> petsQuery = _context.Animals.Include(a => a.Owner);
            if (!string.IsNullOrEmpty(ownerId))
                petsQuery = petsQuery.Where(a => a.OwnerId == ownerId);
            var pets = await petsQuery.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Pets = pets;
            ViewBag.SelectedPetId = petId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Visit model)
        {
            var pet = await _context.Animals.Include(a => a.Owner).FirstOrDefaultAsync(a => a.Id == model.PetId);
            if (pet == null)
                ModelState.AddModelError("PetId", "Musisz wybrać istniejącego pupila.");

            if (!ModelState.IsValid)
            {
                string ownerId = pet?.OwnerId;
                var owners = await _userManager.Users
                    .Where(u => u.Email != "pracownik@punktweterynaryjny.pl")
                    .OrderBy(u => u.Email)
                    .ToListAsync();
                ViewBag.Owners = owners;
                ViewBag.SelectedOwnerId = ownerId;
                var pets = await _context.Animals
                    .Where(a => a.OwnerId == ownerId)
                    .OrderBy(a => a.Name)
                    .ToListAsync();
                ViewBag.Pets = pets;
                ViewBag.SelectedPetId = model.PetId;
                return View(model);
            }

            model.UserId = pet.OwnerId;
            model.User = null;

            _context.Visits.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Dodano nową wizytę!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var visit = await _context.Visits
                .Include(v => v.Pet)
                .ThenInclude(p => p.Owner)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (visit == null) return NotFound();

            var ownerId = visit.Pet?.OwnerId;

            var owners = await _userManager.Users
                .Where(u => u.Email != "pracownik@punktweterynaryjny.pl")
                .OrderBy(u => u.Email)
                .ToListAsync();
            ViewBag.Owners = owners;
            ViewBag.SelectedOwnerId = ownerId;

            var pets = await _context.Animals
                .Where(a => a.OwnerId == ownerId)
                .OrderBy(a => a.Name)
                .ToListAsync();
            ViewBag.Pets = pets;
            ViewBag.SelectedPetId = visit.PetId;

            return View(visit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Visit model)
        {
            if (id != model.Id) return NotFound();

            var visit = await _context.Visits.FindAsync(id);
            if (visit == null) return NotFound();

            var pet = await _context.Animals.FirstOrDefaultAsync(a => a.Id == model.PetId);
            if (pet == null)
            {
                ModelState.AddModelError("PetId", "Nieprawidłowy pupil.");
                return View(model);
            }

            visit.VisitDate = model.VisitDate;
            visit.Description = model.Description;
            visit.Status = model.Status;
            visit.OutVisitAddress = model.OutVisitAddress;
            visit.IsOutVisit = model.IsOutVisit;
            visit.PetId = model.PetId;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Wizyta zaktualizowana!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var visit = await _context.Visits.FindAsync(id);
            if (visit == null) return NotFound();
            _context.Visits.Remove(visit);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Wizyta usunięta!";
            return RedirectToAction(nameof(Index));
        }

		[Authorize(Roles = "Właściciel")]
		[HttpGet]
		public async Task<IActionResult> ExportToCsv()
		{
			var visits = await _context.Visits
				.Include(v => v.Pet)
				.ThenInclude(p => p.Owner)
				.OrderBy(v => v.VisitDate)
				.ToListAsync();

			var sb = new StringBuilder();
			sb.AppendLine("Data,Właściciel,Zwierzę,Opis,Typ wizyty,Status");

			foreach (var v in visits)
			{
				var owner = v.Pet?.Owner?.Email ?? "brak";
				var pet = v.Pet?.Name ?? "brak";
				var desc = v.Description?.Replace(",", " ") ?? "";
				var type = v.IsOutVisit ? "Wyjazdowa" : "Zwykła";

				sb.AppendLine($"{v.VisitDate:yyyy-MM-dd HH:mm},{owner},{pet},{desc},{type},{v.Status}");
			}

			var csv = sb.ToString();
			var bom = Encoding.UTF8.GetPreamble(); // BOM = EF BB BF
			var body = Encoding.UTF8.GetBytes(csv);
			var final = bom.Concat(body).ToArray();

			return File(final, "text/csv; charset=utf-8", "wizyty.csv");

		}
	}
}