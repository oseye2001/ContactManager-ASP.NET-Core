using ContactManager.Data;
using ContactManager.Models;
using ContactManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactManager.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ContactsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<IdentityUser?> CurrentUserAsync()
            => await _userManager.GetUserAsync(User);

        // GET: Contacts
        public async Task<IActionResult> Index(
            string? search,
            int? categorieId,
            string sort = "nom_asc",
            int page = 1,
            int pageSize = 10)
        {
            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var userName = user.UserName!;

            var query = _context.Contacts
                .Include(c => c.Categorie)
                .Where(c => c.UserName == userName)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    (c.Nom + " " + c.Prenom).Contains(search) ||
                    (c.Prenom + " " + c.Nom).Contains(search) ||
                    (c.Courriel ?? "").Contains(search) ||
                    (c.Telephone ?? "").Contains(search));
            }

            if (categorieId.HasValue)
                query = query.Where(c => c.CategorieID == categorieId.Value);

            query = sort switch
            {
                "prenom_asc" => query.OrderBy(c => c.Prenom),
                "prenom_desc" => query.OrderByDescending(c => c.Prenom),
                "date_asc" => query.OrderBy(c => c.DateCreation),
                "date_desc" => query.OrderByDescending(c => c.DateCreation),
                "nom_desc" => query.OrderByDescending(c => c.Nom),
                _ => query.OrderBy(c => c.Nom),
            };

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // ✅ catégories du user seulement
            var userCategories = await _context.Categories
                .Where(x => x.UserId == user.Id)
                .OrderBy(x => x.Nom)
                .ToListAsync();

            var vm = new ContactsIndexVM
            {
                Items = items,
                Search = search,
                CategorieId = categorieId,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Categories = new SelectList(userCategories, "CategorieID", "Nom", categorieId)
            };

            return View(vm);
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var userName = user.UserName!;

            var contact = await _context.Contacts
                .Include(c => c.Categorie)
                .FirstOrDefaultAsync(m => m.ContactID == id && m.UserName == userName);

            if (contact == null) return NotFound();

            return View(contact);
        }

        // GET: Contacts/Create
        public async Task<IActionResult> Create()
        {
            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var userCategories = await _context.Categories
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Nom)
                .ToListAsync();

            // ✅ aucune catégorie POUR CE USER
            if (!userCategories.Any())
            {
                TempData["Warning"] = "Veuillez créer une catégorie avant d'ajouter un contact.";
                return RedirectToAction("Index", "Categories");
            }

            ViewData["CategorieID"] = new SelectList(userCategories, "CategorieID", "Nom");
            return View();
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Prenom,Nom,Adresse,Ville,Province,CodePostal,Telephone,Courriel,CategorieID")]
            Contact contact)
        {
            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            // ✅ sécurité : la catégorie doit appartenir au user
            var catOk = await _context.Categories.AnyAsync(c => c.CategorieID == contact.CategorieID && c.UserId == user.Id);
            if (!catOk)
                ModelState.AddModelError("CategorieID", "Catégorie invalide.");

            if (ModelState.IsValid)
            {
                contact.UserName = user.UserName!;
                contact.DateCreation = DateTime.UtcNow;

                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var userCategories = await _context.Categories
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Nom)
                .ToListAsync();

            ViewData["CategorieID"] = new SelectList(userCategories, "CategorieID", "Nom", contact.CategorieID);
            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var userName = user.UserName!;

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.ContactID == id && c.UserName == userName);

            if (contact == null) return NotFound();

            var userCategories = await _context.Categories
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Nom)
                .ToListAsync();

            ViewData["CategorieID"] = new SelectList(userCategories, "CategorieID", "Nom", contact.CategorieID);
            return View(contact);
        }

        // POST: Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ContactID,Prenom,Nom,Adresse,Ville,Province,CodePostal,Telephone,Courriel,CategorieID")]
            Contact contact)
        {
            if (id != contact.ContactID) return NotFound();

            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var userName = user.UserName!;

            var dbContact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.ContactID == id && c.UserName == userName);

            if (dbContact == null) return NotFound();

            // ✅ sécurité : catégorie appartient au user
            var catOk = await _context.Categories.AnyAsync(c => c.CategorieID == contact.CategorieID && c.UserId == user.Id);
            if (!catOk)
                ModelState.AddModelError("CategorieID", "Catégorie invalide.");

            if (ModelState.IsValid)
            {
                dbContact.Prenom = contact.Prenom;
                dbContact.Nom = contact.Nom;
                dbContact.Adresse = contact.Adresse;
                dbContact.Ville = contact.Ville;
                dbContact.Province = contact.Province;
                dbContact.CodePostal = contact.CodePostal;
                dbContact.Telephone = contact.Telephone;
                dbContact.Courriel = contact.Courriel;
                dbContact.CategorieID = contact.CategorieID;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var userCategories = await _context.Categories
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Nom)
                .ToListAsync();

            ViewData["CategorieID"] = new SelectList(userCategories, "CategorieID", "Nom", contact.CategorieID);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var userName = user.UserName!;

            var contact = await _context.Contacts
                .Include(c => c.Categorie)
                .FirstOrDefaultAsync(m => m.ContactID == id && m.UserName == userName);

            if (contact == null) return NotFound();

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await CurrentUserAsync();
            if (user == null) return Challenge();

            var userName = user.UserName!;

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.ContactID == id && c.UserName == userName);

            if (contact == null) return NotFound();

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
