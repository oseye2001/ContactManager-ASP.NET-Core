using System.Linq;
using System.Threading.Tasks;
using ContactManager.Data;
using ContactManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CategoriesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<IdentityUser?> CurrentUserAsync()
            => await _userManager.GetUserAsync(User);

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Nom)
                .ToListAsync();

            return View(categories);
        }

        // ✅ GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var categorie = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategorieID == id && c.UserId == user.Id);

            if (categorie == null) return NotFound();

            return View(categorie);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom")] Categorie categorie)
        {
            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            categorie.UserId = user.Id;
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Contacts");

            if (!ModelState.IsValid)
                return View(categorie);

            _context.Categories.Add(categorie);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Catégorie créée ✅";
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var categorie = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategorieID == id && c.UserId == user.Id);

            if (categorie == null) return NotFound();

            return View(categorie);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategorieID,Nom")] Categorie categorie)
        {
            if (id != categorie.CategorieID) return NotFound();

            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var dbCategorie = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategorieID == id && c.UserId == user.Id);

            if (dbCategorie == null) return NotFound();

            if (!ModelState.IsValid)
                return View(dbCategorie);

            dbCategorie.Nom = categorie.Nom;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Catégorie modifiée ✅";
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var categorie = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategorieID == id && c.UserId == user.Id);

            if (categorie == null) return NotFound();

            return View(categorie);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var categorie = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategorieID == id && c.UserId == user.Id);

            if (categorie == null) return NotFound();

            _context.Categories.Remove(categorie);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Catégorie supprimée ✅";
            return RedirectToAction(nameof(Index));
        }
    }
}
